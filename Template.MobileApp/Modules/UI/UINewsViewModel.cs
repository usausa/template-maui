namespace Template.MobileApp.Modules.UI;

public sealed partial class UINewsViewModel : AppViewModelBase
{
    // 引っ張って更新で増えた数を出しておく時間
    private static readonly TimeSpan NewArrivalsDuration = TimeSpan.FromSeconds(3);

    private readonly IDispatcher dispatcher;

    [Scope]
    [ObservableProperty]
    public partial UINewsContext Context { get; set; } = default!;

    // タブとページの両方から変わる
    [ObservableProperty]
    public partial int SelectedIndex { get; set; }

    // 表示の前 (OnNavigatingToAsync) に設定する
    [ObservableProperty]
    public partial UINewsPage SelectedPage { get; set; } = default!;

    // ページのスクロールの位置 (タブの下線が追う)
    [ObservableProperty]
    public partial double PagePosition { get; set; }

    public IObserveCommand RefreshCommand { get; }

    public IObserveCommand MoreCommand { get; }

    public IObserveCommand ArticleCommand { get; }

    public IObserveCommand BreakingCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UINewsViewModel(IDispatcher dispatcher)
    {
        this.dispatcher = dispatcher;

        // ページの中身はスワイプの間には作らず、選んだ後に読み込む (作る処理が重いとスワイプが止まる)
        SubscribeSelectedIndex(_ => Select());

        RefreshCommand = MakeAsyncCommand<UINewsPage>(RefreshAsync);
        MoreCommand = MakeDelegateCommand<UINewsPage>(CommandMode.Simple, x => _ = Context.LoadMoreAsync(x));
        ArticleCommand = MakeAsyncCommand<UINewsItem>(OpenAsync);
        BreakingCommand = MakeAsyncCommand(OpenBreakingAsync);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            SelectedPage = Context.Pages[SelectedIndex];
        }

        return Task.CompletedTask;
    }

    // 記事から戻ったときは、文脈が既読と保存を一覧に反映している
    public override Task OnNavigatedToAsync(INavigationContext context) =>
        Context.LoadAsync(Context.Pages[SelectedIndex]);

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    // 増えた数はしばらくして消す
    private async Task RefreshAsync(UINewsPage page)
    {
        await Context.RefreshAsync(page);
        dispatcher.DispatchDelayed(NewArrivalsDuration, () => page.NewArrivals = 0);
    }

    private Task OpenAsync(UINewsItem item)
    {
        Context.Open(item);
        return Navigator.PushAsync(ViewId.UINewsDetail);
    }

    private Task OpenBreakingAsync()
    {
        Context.OpenBreaking();
        return Navigator.PushAsync(ViewId.UINewsDetail);
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void Select()
    {
        SelectedPage = Context.Pages[SelectedIndex];
        _ = Context.LoadAsync(SelectedPage);
    }
}
