namespace Template.MobileApp.Modules.Device;

using Plugin.Maui.Audio;

using Smart.Maui.Input;

public sealed class DeviceAudioViewModel : AppViewModelBase
{
    private readonly IFileSystem fileSystem;

    private readonly IAudioManager audioManager;

    public IAudioPlayer? AudioPlayer { get; set; }

    public IObserveCommand PlayCommand { get; }
    public IObserveCommand PauseCommand { get; }
    public IObserveCommand StopCommand { get; }

    public DeviceAudioViewModel(
        IFileSystem fileSystem,
        IAudioManager audioManager)
    {
        this.fileSystem = fileSystem;
        this.audioManager = audioManager;

        PlayCommand = MakeDelegateCommand(Play);
        PauseCommand = new DelegateCommand(Pause);
        StopCommand = new DelegateCommand(Stop);
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            AudioPlayer = audioManager.CreatePlayer(await fileSystem.OpenAppPackageFileAsync("Sample.mp3"));
            Disposables.Add(AudioPlayer);
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    private void Play()
    {
        if (AudioPlayer is null)
        {
            return;
        }

        AudioPlayer.Stop();
        AudioPlayer.Play();
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
    }

    private void Stop()
    {
        AudioPlayer?.Stop();
    }
}
