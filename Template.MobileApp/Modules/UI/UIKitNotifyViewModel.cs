namespace Template.MobileApp.Modules.UI;

public sealed partial class UIKitNotifyItem : ObservableObject
{
    public string Icon { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Time { get; init; } = string.Empty;

    [ObservableProperty]
    public partial bool IsUnread { get; set; }
}

public sealed class UIKitNotifyViewModel : AppViewModelBase
{
    public IReadOnlyList<UIKitNotifyItem> Notifications { get; } =
    [
        new() { Icon = Fonts.MaterialIcons.Local_offer, Title = "50% off today", Description = "Spring sale ends tonight. Tap to explore.", Time = "5m", IsUnread = true },
        new() { Icon = Fonts.MaterialIcons.Local_shipping, Title = "Your order is out for delivery", Description = "Estimated arrival: 14:30 - 16:00.", Time = "1h", IsUnread = true },
        new() { Icon = Fonts.MaterialIcons.Mail, Title = "New message from Anna", Description = "Hey! Are we still meeting tomorrow?", Time = "3h", IsUnread = true },
        new() { Icon = Fonts.MaterialIcons.Star, Title = "You earned a badge", Description = "You've completed 10 orders this month.", Time = "1d" },
        new() { Icon = Fonts.MaterialIcons.Notifications, Title = "Weekly summary", Description = "Check what you achieved this week.", Time = "2d" }
    ];

    // タップで既読化
    public IObserveCommand ReadCommand { get; }

    public UIKitNotifyViewModel()
    {
        ReadCommand = MakeDelegateCommand<UIKitNotifyItem>(x => x.IsUnread = false);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIKitDash);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
