namespace Template.MobileApp.Modules.UI;

public sealed partial class UIShopContext : ObservableObject
{
    private readonly TimeProvider timeProvider;

    public ShopStore Store { get; } = new(ShopSample.LoadProducts(), ShopSample.LoadCart());

    [ObservableProperty]
    public partial ShopItem Selected { get; set; } = default!;

    public UIShopContext(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public ShopOrder Checkout() => Store.Checkout(timeProvider.GetLocalNow().Date);
}
