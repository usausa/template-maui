namespace Template.MobileApp.Modules.UI;

public sealed partial class UIKitSettingItem : ObservableObject
{
    public string Icon { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public bool IsSwitch { get; init; }
    public string Detail { get; init; } = string.Empty;

    [ObservableProperty]
    public partial bool IsOn { get; set; }
}

public sealed class UIKitSettingViewModel : AppViewModelBase
{
    public IReadOnlyList<UIKitSettingItem> AccountItems { get; } =
    [
        new() { Icon = Fonts.MaterialIcons.Person, Title = "Profile", Detail = "Anna Walker" },
        new() { Icon = Fonts.MaterialIcons.Lock, Title = "Password", Detail = "Change" },
        new() { Icon = Fonts.MaterialIcons.Email, Title = "Email", Detail = "anna@example.com" }
    ];

    public IReadOnlyList<UIKitSettingItem> PreferenceItems { get; } =
    [
        new() { Icon = Fonts.MaterialIcons.Notifications, Title = "Push notifications", IsSwitch = true, IsOn = true },
        new() { Icon = Fonts.MaterialIcons.Mail, Title = "Email updates", IsSwitch = true, IsOn = false },
        new() { Icon = Fonts.MaterialIcons.Dark_mode, Title = "Dark mode", IsSwitch = true, IsOn = false }
    ];

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIKitDash);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
