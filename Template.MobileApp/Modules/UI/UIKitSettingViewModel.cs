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

public sealed partial class UIKitSettingViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    public IReadOnlyList<UIKitSettingItem> AccountItems { get; } =
    [
        new() { Icon = Fonts.MaterialIcons.Person, Title = "プロフィール", Detail = "うさうささん" },
        new() { Icon = Fonts.MaterialIcons.Lock, Title = "パスワード", Detail = "変更" },
        new() { Icon = Fonts.MaterialIcons.Email, Title = "メールアドレス", Detail = "usausa@example.com" }
    ];

    public IReadOnlyList<UIKitSettingItem> PreferenceItems { get; } =
    [
        new() { Icon = Fonts.MaterialIcons.Notifications, Title = "プッシュ通知", IsSwitch = true, IsOn = true },
        new() { Icon = Fonts.MaterialIcons.Mail, Title = "メールでのお知らせ", IsSwitch = true, IsOn = false },
        new() { Icon = Fonts.MaterialIcons.Dark_mode, Title = "ダークモード", IsSwitch = true, IsOn = false }
    ];

    [ObservableProperty]
    public partial double TextSize { get; set; } = 14;

    // 画面のアイコンとスイッチの色
    [ObservableProperty]
    public partial Color ThemeColor { get; set; } = Color.FromArgb("#2196F3");

    [ObservableProperty]
    public partial bool IsColorOpen { get; set; }

    public Version Version { get; }

    public IObserveCommand ColorCommand { get; }

    public IObserveCommand LogoutCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIKitSettingViewModel(
        IDialog dialog,
        IAppInfo appInfo)
    {
        this.dialog = dialog;
        Version = appInfo.Version;

        ColorCommand = MakeDelegateCommand(() => IsColorOpen = !IsColorOpen);
        LogoutCommand = MakeAsyncCommand(LogoutAsync);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIKitDash);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    // ログアウトしたら、はじめにの画面へ
    private async Task LogoutAsync()
    {
        if (await dialog.ConfirmAsync("ログアウトしますか?"))
        {
            await Navigator.ForwardAsync(ViewId.UIKitOnboard);
        }
    }
}
