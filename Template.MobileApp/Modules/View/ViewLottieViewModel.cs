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

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public ViewLottieViewModel()
    {
        PlayPauseCommand = MakeDelegateCommand(() => IsAnimationEnabled = !IsAnimationEnabled);
        ResetCommand = MakeDelegateCommand(() => Progress = TimeSpan.Zero);
        SeekCommand = MakeDelegateCommand<double>(x => Seek(TimeSpan.FromSeconds(x)));

        ScrubCommand = MakeDelegateCommand<double>(CommandMode.Simple, x =>
        {
            IsAnimationEnabled = false;
            Seek(TimeSpan.FromTicks((long)(Duration.Ticks * x)));
        });
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        IsAnimationEnabled = true;
        return Task.CompletedTask;
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

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    // 繰り返す SKLottieView は、止めたまま最後の位置にすると、先頭に戻す値と最後の値を交互に設定し続けて固まる。最後の 1 tick 手前までにする
    private void Seek(TimeSpan value) =>
        Progress = TimeSpan.FromTicks(Math.Clamp(value.Ticks, 0, Math.Max(0, Duration.Ticks - 1)));
}
