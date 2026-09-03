namespace Template.MobileApp.Modules.Sample;

public sealed partial class SampleMediaViewModel : AppViewModelBase
{
    private readonly IDispatcherTimer timer;

    public MediaController Controller { get; } = new();

    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsControlBarVisible { get; set; }

    public ICommand TogglePlayCommand { get; }

    public ICommand SeekCommand { get; }

    public ICommand ToggleControlBarCommand { get; }

    public SampleMediaViewModel(IDispatcher dispatcher)
    {
        // 再生が始まるまではローディング表示にする
        IsLoading = true;

        Controller.PlayingChanged = x => IsPlaying = x;
        Controller.LoadingChanged = x => IsLoading = x;

        TogglePlayCommand = MakeDelegateCommand(() =>
        {
            Controller.TogglePlay();
            RestartHideTimer();
        });
        SeekCommand = MakeDelegateCommand<double>(x =>
        {
            Controller.SeekTo(x);
            RestartHideTimer();
        });
        ToggleControlBarCommand = MakeDelegateCommand(() =>
        {
            IsControlBarVisible = !IsControlBarVisible;
            RestartHideTimer();
        });

        // コントロールバーは表示から 3 秒後に自動で隠す
        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(3);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ =>
        {
            timer.Stop();
            IsControlBarVisible = false;
        }));
    }

    private void RestartHideTimer()
    {
        timer.Stop();
        if (IsControlBarVisible)
        {
            timer.Start();
        }
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
