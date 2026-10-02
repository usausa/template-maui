namespace Template.MobileApp.Modules.UI;

public sealed partial class UINewsDetailViewModel : AppViewModelBase
{
    private static readonly double[] FontSizes = [14, 16, 18, 20, 22];

    private readonly IShare share;

    [Scope]
    [ObservableProperty]
    public partial UINewsContext Context { get; set; } = default!;

    [ObservableProperty(NotifyAlso = [nameof(BodyFontSize)])]
    public partial int FontSizeIndex { get; set; } = 1;

    public double BodyFontSize => FontSizes[FontSizeIndex];

    // 読んだ量 (0〜1)
    [ObservableProperty]
    public partial double ReadProgress { get; set; }

    public IObserveCommand SaveCommand { get; }

    public IObserveCommand ShareCommand { get; }

    public IObserveCommand SmallerCommand { get; }

    public IObserveCommand LargerCommand { get; }

    public IObserveCommand RelatedCommand { get; }

    public IObserveCommand ProgressCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UINewsDetailViewModel(IShare share)
    {
        this.share = share;

#pragma warning disable IDE0200
        SaveCommand = MakeDelegateCommand(() => Context.ToggleSave());
#pragma warning restore IDE0200
        ShareCommand = MakeAsyncCommand(ShareAsync);
        SmallerCommand = MakeDelegateCommand(() => FontSizeIndex--, () => FontSizeIndex > 0);
        LargerCommand = MakeDelegateCommand(() => FontSizeIndex++, () => FontSizeIndex < FontSizes.Length - 1);
        RelatedCommand = MakeDelegateCommand<NewsArticle>(ShowRelated);
        ProgressCommand = MakeDelegateCommand<double>(CommandMode.Simple, x => ReadProgress = x);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    // 一覧は Push の前の状態 (タブと位置) のまま残っている
    protected override Task OnNotifyBackAsync() => Navigator.PopAsync();

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ShowRelated(NewsArticle article)
    {
        Context.ShowRelated(article);
        ReadProgress = 0;
    }

    private Task ShareAsync() =>
        share.RequestAsync(new ShareTextRequest
        {
            Title = Context.Article.Title,
            Text = $"{Context.Article.Title}\n{Context.Article.Summary}"
        });
}
