namespace Template.MobileApp.Modules.View;

// ref https://lottiefiles.com/
public sealed partial class ViewLottieViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial bool IsAnimationEnabled { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(DurationSeconds)])]
    public partial TimeSpan Duration { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(ProgressSeconds)])]
    public partial TimeSpan Progress { get; set; }

    public double ProgressSeconds => Progress.TotalSeconds;

    public double DurationSeconds => Math.Max(0.1d, Duration.TotalSeconds);

    public IObserveCommand PlayPauseCommand { get; }
    public IObserveCommand ResetCommand { get; }
    public IObserveCommand SeekCommand { get; }
    public IObserveCommand ScrubCommand { get; }

    public ViewLottieViewModel()
    {
        PlayPauseCommand = MakeDelegateCommand(() => IsAnimationEnabled = !IsAnimationEnabled);
        ResetCommand = MakeDelegateCommand(() => Progress = TimeSpan.Zero);
        SeekCommand = MakeDelegateCommand<double>(x => Progress = TimeSpan.FromSeconds(x));

        // スクロール連動 / 長押し進行 (B-7): 0-1 の比率で再生位置を進める
        ScrubCommand = MakeDelegateCommand<double>(x =>
        {
            IsAnimationEnabled = false;
            Progress = Duration.Ticks > 0 ? TimeSpan.FromTicks((long)(Duration.Ticks * x)) : TimeSpan.Zero;
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction3()
    {
        Progress = TimeSpan.Zero;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        IsAnimationEnabled = !IsAnimationEnabled;
        return Task.CompletedTask;
    }
}
