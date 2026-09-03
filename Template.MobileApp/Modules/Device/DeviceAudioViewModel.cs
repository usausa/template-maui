namespace Template.MobileApp.Modules.Device;

using Plugin.Maui.Audio;

using Smart.Maui.Input;

public sealed partial class DeviceAudioViewModel : AppViewModelBase
{
    private readonly IFileSystem fileSystem;

    private readonly IAudioManager audioManager;

    private IDisposable? polling;

    public IAudioPlayer? AudioPlayer { get; set; }

    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(PositionRatio), nameof(PositionText)])]
    public partial double Position { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(PositionRatio), nameof(DurationText)])]
    public partial double Duration { get; set; }

    public double PositionRatio => Duration > 0 ? Math.Clamp(Position / Duration, 0d, 1d) : 0d;

    public string PositionText => TimeSpan.FromSeconds(Position).ToString(@"m\:ss", CultureInfo.InvariantCulture);

    public string DurationText => TimeSpan.FromSeconds(Duration).ToString(@"m\:ss", CultureInfo.InvariantCulture);

    public IObserveCommand PlayCommand { get; }
    public IObserveCommand PauseCommand { get; }
    public IObserveCommand StopCommand { get; }
    public IObserveCommand SeekCommand { get; }

    public DeviceAudioViewModel(
        IFileSystem fileSystem,
        IAudioManager audioManager)
    {
        this.fileSystem = fileSystem;
        this.audioManager = audioManager;

        PlayCommand = MakeDelegateCommand(Play);
        PauseCommand = new DelegateCommand(Pause);
        StopCommand = new DelegateCommand(Stop);
        SeekCommand = MakeDelegateCommand<double>(Seek);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            polling?.Dispose();
            polling = null;
        }

        base.Dispose(disposing);
    }

    public override async Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            AudioPlayer = audioManager.CreatePlayer(await fileSystem.OpenAppPackageFileAsync(Path.Combine("Sounds", "Sample.mp3")));
            Disposables.Add(AudioPlayer);
        }
    }

    // IAudioPlayer は変更通知を持たないため再生位置・状態をポーリングで反映する (スタック退避中は停止)
    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        // 二重呼び出しで前回の購読が解除できなくなるのを防ぐ
        polling?.Dispose();
        polling = Observable.Interval(TimeSpan.FromMilliseconds(250))
            .ObserveOnCurrentContext()
            .Subscribe(_ => UpdateState());
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        polling?.Dispose();
        polling = null;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    private void UpdateState()
    {
        if (AudioPlayer is null)
        {
            return;
        }

        IsPlaying = AudioPlayer.IsPlaying;
        Duration = AudioPlayer.Duration;
        Position = AudioPlayer.CurrentPosition;
    }

    private void Play()
    {
        if (AudioPlayer is null)
        {
            return;
        }

        AudioPlayer.Stop();
        AudioPlayer.Play();
        UpdateState();
    }

    private void Pause()
    {
        if (AudioPlayer is null)
        {
            return;
        }

        if (AudioPlayer.IsPlaying)
        {
            AudioPlayer.Pause();
        }
        else
        {
            AudioPlayer.Play();
        }

        UpdateState();
    }

    private void Stop()
    {
        AudioPlayer?.Stop();
        UpdateState();
    }

    private void Seek(double ratio)
    {
        if (AudioPlayer is null)
        {
            return;
        }

        AudioPlayer.Seek(ratio * Duration);
        UpdateState();
    }
}
