namespace Template.MobileApp.Modules.UI;

public sealed partial class UICartViewModel : AppViewModelBase
{
    [Scope]
    [ObservableProperty]
    public partial UIShopContext Context { get; set; } = default!;

    public string Coupon { get; } = "スプリングセール −10%";

    public string Points { get; } = "12,540 pt 利用可能";

    // 確定した注文 (IsCompleted の後に表示する)
    [ObservableProperty]
    public partial ShopOrder Order { get; set; } = default!;

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    public IObserveCommand BackCommand { get; }

    public IObserveCommand IncrementCommand { get; }

    public IObserveCommand DecrementCommand { get; }

    public IObserveCommand RemoveCommand { get; }

    public IObserveCommand CheckoutCommand { get; }

    public IObserveCommand ContinueCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UICartViewModel()
    {
        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
#pragma warning disable IDE0200
        IncrementCommand = MakeDelegateCommand<ShopCartItem>(x => Context.Store.Increase(x));
        DecrementCommand = MakeDelegateCommand<ShopCartItem>(x => Context.Store.Decrease(x));
        RemoveCommand = MakeDelegateCommand<ShopCartItem>(x => Context.Store.Remove(x));
#pragma warning restore IDE0200
        CheckoutCommand = MakeDelegateCommand(() =>
        {
            Order = Context.Checkout();
            IsCompleted = true;
        });
        // 一覧の画面まで戻る
        ContinueCommand = MakeAsyncCommand(() => Navigator.PopAsync(Navigator.StackedCount - 1));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.PopAsync();

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
