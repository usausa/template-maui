namespace Template.MobileApp.Modules.UI;

public sealed class UIKitDashMetric
{
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
}

public sealed class UIKitDashViewModel : AppViewModelBase
{
    public string Greeting { get; } = "Good Morning,";
    public string UserName { get; } = "Anna";

    public IReadOnlyList<UIKitDashMetric> Metrics { get; } =
    [
        new() { Title = "Steps", Value = "7,852", Unit = "/ 10,000", Icon = Fonts.MaterialIcons.Directions_walk },
        new() { Title = "Heart Rate", Value = "72", Unit = "bpm", Icon = Fonts.MaterialIcons.Favorite },
        new() { Title = "Calories", Value = "412", Unit = "kcal", Icon = Fonts.MaterialIcons.Local_fire_department },
        new() { Title = "Sleep", Value = "7.4", Unit = "hours", Icon = Fonts.MaterialIcons.Bedtime }
    ];

    public IObserveCommand NotifyCommand { get; }

    public IObserveCommand SettingCommand { get; }

    public IObserveCommand OnboardCommand { get; }

    public IObserveCommand TrackingCommand { get; }

    public UIKitDashViewModel()
    {
        NotifyCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitNotify));
        SettingCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitSetting));
        OnboardCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitOnboard));
        TrackingCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitTracking));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
