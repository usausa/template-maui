namespace Template.MobileApp.Modules.UI;

// 一覧・商品・カートが共有する (一覧を開いている間)
public sealed partial class UIShopContext : ObservableObject
{
    private readonly TimeProvider timeProvider;

    public ShopStore Store { get; } = new(ShopSample.LoadProducts(), ShopSample.LoadCart());

    // 商品の画面の商品 (Push の前に設定する)
    [ObservableProperty]
    public partial ShopItem Selected { get; set; } = default!;

    public UIShopContext(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public ShopOrder Checkout() => Store.Checkout(timeProvider.GetLocalNow().Date);
}
