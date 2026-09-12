namespace Template.MobileApp.Modules.UI;

public sealed class UIKitOnboardPage
{
    public string Image { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public sealed class UIKitOnboardViewModel : AppViewModelBase
{
    public IReadOnlyList<UIKitOnboardPage> Pages { get; } =
    [
        new() { Image = "onboard01.jpg", Title = "ようこそ", Description = "モバイルアプリのための美しいデザインキットを体験しましょう。" },
        new() { Image = "onboard02.jpg", Title = "いつでもつながる", Description = "タスクや友だち、コンテンツを端末間で同期します。" },
        new() { Image = "onboard03.jpg", Title = "さあ、始めよう", Description = "サインインして、あなた専用の体験をお楽しみください。" }
    ];

    // スキップ / 始める はどちらもダッシュボードへ
    public IObserveCommand CompleteCommand { get; }

    public UIKitOnboardViewModel()
    {
        CompleteCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitDash));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIKitDash);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
