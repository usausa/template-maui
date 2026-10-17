namespace Template.MobileApp.Modules.UI;

public sealed class UIStreamSection
{
    public required StreamShelf Shelf { get; init; }

    public required IReadOnlyList<StreamItem> Items { get; init; }

    public int Delay { get; init; }
}

public sealed partial class UIStreamViewModel : AppViewModelBase
{
    private const int AdvanceSeconds = 5;

    // Field

    private readonly IDispatcherTimer timer;

    private int idleSeconds;

    // Property

    [Scope]
    [ObservableProperty]
    public partial UIStreamContext Context { get; set; } = default!;

    [ObservableProperty]
    public partial IReadOnlyList<StreamItem> Featured { get; set; } = [];

    [ObservableProperty]
    public partial int FeaturedPosition { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<StreamItem> Continue { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<UIStreamSection> Sections { get; set; } = [];

    // Command

    public IObserveCommand DetailCommand { get; }

    public IObserveCommand MyListCommand { get; }

    public IObserveCommand CurrentChangedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIStreamViewModel(IDispatcher dispatcher)
    {
        DetailCommand = MakeAsyncCommand<StreamItem>(OpenAsync);
        MyListCommand = MakeDelegateCommand<StreamItem>(static x => x.InMyList = !x.InMyList);
        CurrentChangedCommand = MakeDelegateCommand(() => idleSeconds = 0);

        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => Advance()));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            var library = Context.Library;
            Featured = library.Featured();
            Continue = library.Continue();
            Sections =
            [
                new() { Shelf = StreamShelf.TopRated, Items = library.Shelf(StreamShelf.TopRated), Delay = 0 },
                new() { Shelf = StreamShelf.Original, Items = library.Shelf(StreamShelf.Original), Delay = 80 },
                new() { Shelf = StreamShelf.Trending, Items = library.Shelf(StreamShelf.Trending), Delay = 160 },
                new() { Shelf = StreamShelf.Action, Items = library.Shelf(StreamShelf.Action), Delay = 240 }
            ];
        }

        return Task.CompletedTask;
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        idleSeconds = 0;
        timer.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private Task OpenAsync(StreamItem item)
    {
        timer.Stop();
        Context.Open(item);
        return Navigator.PushAsync(ViewId.UIStreamDetail);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void Advance()
    {
        idleSeconds++;
        if (idleSeconds >= AdvanceSeconds)
        {
            idleSeconds = 0;
            FeaturedPosition = (FeaturedPosition + 1) % Featured.Count;
        }
    }
}
