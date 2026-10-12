namespace Template.MobileApp.Modules.UI;

public sealed partial class UIItemViewModel : AppViewModelBase
{
    private const int MaxQuantity = 99;

    // カートに入れた後の表示を出しておく時間
    private static readonly TimeSpan AddedDuration = TimeSpan.FromSeconds(1.5);

    private readonly IDispatcher dispatcher;

    [Scope]
    [ObservableProperty]
    public partial UIShopContext Context { get; set; } = default!;

    // 表示の前 (OnNavigatingToAsync) に設定する
    [ObservableProperty]
    public partial string SelectedVariant { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int Quantity { get; set; } = 1;

    [ObservableProperty]
    public partial bool IsAdded { get; set; }

    public IObserveCommand BackCommand { get; }

    public IObserveCommand CartCommand { get; }

    public IObserveCommand FavoriteCommand { get; }

    public IObserveCommand IncrementCommand { get; }

    public IObserveCommand DecrementCommand { get; }

    public IObserveCommand AddCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIItemViewModel(IDispatcher dispatcher)
    {
        this.dispatcher = dispatcher;

        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
        CartCommand = MakeAsyncCommand(() => Navigator.PushAsync(ViewId.UICart));
        FavoriteCommand = MakeDelegateCommand(() => Context.Selected.IsFavorite = !Context.Selected.IsFavorite);
        IncrementCommand = MakeDelegateCommand(() => Quantity = Math.Min(MaxQuantity, Quantity + 1));
        DecrementCommand = MakeDelegateCommand(() => Quantity = Math.Max(1, Quantity - 1));
        AddCommand = MakeDelegateCommand(Add);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            var variants = Context.Selected.Product.Variants;
            SelectedVariant = variants.Count > 0 ? variants[0] : string.Empty;
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.PopAsync();

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Add()
    {
        Context.Store.AddToCart(Context.Selected, SelectedVariant, Quantity);
        Quantity = 1;
        IsAdded = true;
        dispatcher.DispatchDelayed(AddedDuration, () => IsAdded = false);
    }
}
