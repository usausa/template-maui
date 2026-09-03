namespace Template.MobileApp.Modules.View;

public sealed partial class ViewEffectViewModel : AppViewModelBase
{
    // バッジは (空)/通常/上限超え の 3 状態を巡回して確認する
    private static readonly int[] BadgeSteps = [0, 5, 120];

    private int badgeIndex;

    [ObservableProperty]
    public partial int ReplayCount { get; set; }

    [ObservableProperty]
    public partial int BadgeCount { get; set; }

    [ObservableProperty]
    public partial double Amount { get; set; }

    [ObservableProperty]
    public partial int FeedbackCount { get; set; }

    [ObservableProperty]
    public partial bool WavePulseEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool ConfettiEnabled { get; set; }

    [ObservableProperty]
    public partial int LongPressCount { get; set; }

    public IObserveCommand ReplayCommand { get; }

    public IObserveCommand BadgePrevCommand { get; }

    public IObserveCommand BadgeNextCommand { get; }

    public IObserveCommand AmountCommand { get; }

    public IObserveCommand FeedbackCommand { get; }

    public IObserveCommand CelebrateCommand { get; }

    public IObserveCommand LongPressCommand { get; }

    public ViewEffectViewModel()
    {
        ReplayCommand = MakeDelegateCommand(() => ReplayCount++);
        BadgePrevCommand = MakeDelegateCommand(() => UpdateBadge(-1));
        BadgeNextCommand = MakeDelegateCommand(() => UpdateBadge(1));
        AmountCommand = MakeDelegateCommand(UpdateAmount);
        FeedbackCommand = MakeDelegateCommand(() => FeedbackCount++);
        CelebrateCommand = MakeDelegateCommand(() => ConfettiEnabled = !ConfettiEnabled);
        LongPressCommand = MakeDelegateCommand(() => LongPressCount++);
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (Amount <= 0)
        {
            Amount = 24800;
        }
        return Task.CompletedTask;
    }

    private void UpdateBadge(int direction)
    {
        badgeIndex = (badgeIndex + direction + BadgeSteps.Length) % BadgeSteps.Length;
        BadgeCount = BadgeSteps[badgeIndex];
    }

#pragma warning disable CA5394
    private void UpdateAmount()
    {
        Amount = Random.Shared.Next(1, 100) * 1000d;
    }
#pragma warning restore CA5394

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
