namespace Template.MobileApp.Modules.UI;

public sealed class UIKitOnboardPage
{
    public string Image { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    // 絵の地と同じ色で、ページ全体を塗る
    public Color Background { get; init; } = Colors.White;
}

public sealed partial class UIKitOnboardViewModel : AppViewModelBase
{
    public IReadOnlyList<UIKitOnboardPage> Pages { get; } =
    [
        new() { Image = "onboard01.jpg", Title = "ようこそ", Description = "モバイルアプリのための美しいデザインキットを体験しましょう。", Background = Color.FromArgb("#C9E8FC") },
        new() { Image = "onboard02.jpg", Title = "いつでもつながる", Description = "タスクや友だち、コンテンツを端末間で同期します。", Background = Color.FromArgb("#F2F8EE") },
        new() { Image = "onboard03.jpg", Title = "さあ、始めよう", Description = "サインインして、あなた専用の体験をお楽しみください。", Background = Color.FromArgb("#FCECD9") }
    ];

    [ObservableProperty]
    public partial int Position { get; set; }

    public IObserveCommand NextCommand { get; }

    // スキップ / 始める はどちらもダッシュボードへ
    public IObserveCommand CompleteCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIKitOnboardViewModel()
    {
        NextCommand = MakeDelegateCommand(() => Position = Math.Min(Position + 1, Pages.Count - 1));
        CompleteCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitDash));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIKitDash);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
