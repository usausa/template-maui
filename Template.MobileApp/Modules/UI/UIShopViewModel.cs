namespace Template.MobileApp.Modules.UI;

public sealed class UIShopItem
{
    public string Title { get; init; } = string.Empty;
    public string Price { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
}

public sealed partial class UIShopViewModel : AppViewModelBase
{
    private static readonly IReadOnlyList<UIShopItem> AllItems =
    [
        new() { Title = "メカニカルキーボード", Price = "¥19,800", Image = "product_gear01.jpg" },
        new() { Title = "ワイヤレスマウス", Price = "¥8,900", Image = "product_gear02.jpg" },
        new() { Title = "ノイズキャンセリングヘッドセット", Price = "¥14,800", Image = "product_gear03.jpg" },
        new() { Title = "4K Web カメラ", Price = "¥11,800", Image = "product_gear04.jpg" },
        new() { Title = "ポータブル SSD 1TB", Price = "¥16,800", Image = "product_gear05.jpg" },
        new() { Title = "USB-C ドック", Price = "¥27,800", Image = "product_gear06.jpg" }
    ];

    public string Greeting { get; } = "こんにちは、アンナさん";

    public string SubGreeting { get; } = "デスク周りをアップグレード";

    // 検索は All Items をタイトルの部分一致で絞り込む
    [ObservableProperty(NotifyAlso = [nameof(Items)])]
    public partial string SearchText { get; set; } = string.Empty;

    public IObserveCommand ItemCommand { get; }

    public UIShopViewModel()
    {
        ItemCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIItem));
    }

    public IReadOnlyList<UIShopItem> Popular { get; } =
    [
        new() { Title = "スリムノート PC 14", Price = "¥179,800", Image = "product_device01.jpg" },
        new() { Title = "4K モニター 27", Price = "¥64,800", Image = "product_device02.jpg" },
        new() { Title = "コンパクトデスクトップ", Price = "¥129,800", Image = "product_device03.jpg" }
    ];

    public IReadOnlyList<UIShopItem> Items =>
        String.IsNullOrWhiteSpace(SearchText)
            ? AllItems
            : AllItems.Where(x => x.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
