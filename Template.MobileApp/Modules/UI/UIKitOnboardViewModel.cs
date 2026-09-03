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
        new() { Image = "social_background.png", Title = "Welcome", Description = "Discover the most beautiful design kit for mobile apps." },
        new() { Image = "social_background.png", Title = "Stay Connected", Description = "Sync your tasks, friends, and content across devices." },
        new() { Image = "social_background.png", Title = "Get Started", Description = "Sign in to enjoy the full personalized experience." }
    ];

    // Skip / Get Started はどちらもダッシュボードへ
    public IObserveCommand CompleteCommand { get; }

    public UIKitOnboardViewModel()
    {
        CompleteCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitDash));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIKitDash);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
