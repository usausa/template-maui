namespace Template.MobileApp.Modules.UI;

public sealed partial class UINewsViewModel : AppViewModelBase
{
    private static readonly TimeSpan NewArrivalsDuration = TimeSpan.FromSeconds(3);

    private readonly IDispatcher dispatcher;

    [Scope]
    [ObservableProperty]
    public partial UINewsContext Context { get; set; } = default!;

    [ObservableProperty]
    public partial int SelectedIndex { get; set; }

    [ObservableProperty]
    public partial UINewsPage SelectedPage { get; set; } = default!;

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

    public override Task OnNavigatedToAsync(INavigationContext context) =>
        Context.LoadAsync(Context.Pages[SelectedIndex]);

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

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
