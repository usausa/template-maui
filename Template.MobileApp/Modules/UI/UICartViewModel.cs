namespace Template.MobileApp.Modules.UI;

public sealed partial class UICartItem : ObservableObject
{
    public string Title { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Delay { get; init; }

    [ObservableProperty]
    public partial int Quantity { get; set; }

    public string PriceText => String.Format(CultureInfo.CurrentCulture, "¥{0:N0}", UnitPrice);
}

public sealed partial class UICartViewModel : AppViewModelBase
{
    private const decimal DiscountRate = 0.10m;

    public IReadOnlyList<UICartItem> Items { get; }

    [ObservableProperty]
    public partial int ItemCount { get; set; }

    [ObservableProperty]
    public partial decimal Subtotal { get; set; }

    [ObservableProperty]
    public partial decimal Discount { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(TotalValue)])]
    public partial decimal Total { get; set; }

    // CountUp 演出用(double)
    public double TotalValue => (double)Total;

    public string Coupon { get; } = "スプリングセール −10%";

    public string Points { get; } = "12,540 pt 利用可能";

    public IObserveCommand IncrementCommand { get; }
    public IObserveCommand DecrementCommand { get; }
    public IObserveCommand CheckoutCommand { get; }

    public UICartViewModel(IDialog dialog)
    {
        Items =
        [
            new() { Title = "メカニカルキーボード", UnitPrice = 19800m, Quantity = 1, Image = "product_gear01.jpg", Delay = 0 },
            new() { Title = "ワイヤレスマウス", UnitPrice = 8900m, Quantity = 2, Image = "product_gear02.jpg", Delay = 80 },
            new() { Title = "ノイズキャンセリングヘッドセット", UnitPrice = 14800m, Quantity = 1, Image = "product_gear03.jpg", Delay = 160 }
        ];

        IncrementCommand = MakeDelegateCommand<UICartItem>(item =>
        {
            item.Quantity = Math.Min(99, item.Quantity + 1);
            Recalculate();
        });
        DecrementCommand = MakeDelegateCommand<UICartItem>(item =>
        {
            item.Quantity = Math.Max(1, item.Quantity - 1);
            Recalculate();
        });
        CheckoutCommand = MakeAsyncCommand(async () =>
            await dialog.InformationAsync(String.Format(CultureInfo.CurrentCulture, "お会計が完了しました。\n{0} 点 / ¥{1:N0}", ItemCount, Total)));

        Recalculate();
    }

    private void Recalculate()
    {
        ItemCount = Items.Sum(static x => x.Quantity);
        Subtotal = Items.Sum(static x => x.UnitPrice * x.Quantity);
        Discount = Math.Round(Subtotal * DiscountRate, 0);
        Total = Subtotal - Discount;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIItem);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
