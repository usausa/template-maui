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
        new() { Title = "Aqua Serum", Price = "$ 24.00", Image = "usa1_face.jpg" },
        new() { Title = "Velvet Lip", Price = "$ 18.50", Image = "usa2_face.jpg" },
        new() { Title = "Glow Cream", Price = "$ 32.00", Image = "usa3_face.jpg" },
        new() { Title = "Pure Mist", Price = "$ 12.00", Image = "usa4_face.jpg" },
        new() { Title = "Silky Mask", Price = "$ 22.00", Image = "usa5_face.jpg" },
        new() { Title = "Petal Blush", Price = "$ 15.80", Image = "usa6_face.jpg" }
    ];

    public string Greeting { get; } = "Hello, Anna!";

    public string SubGreeting { get; } = "Discover your style";

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
        new() { Title = "Velvet Dress", Price = "$ 89.00", Image = "usa1_full.jpg" },
        new() { Title = "Denim Jacket", Price = "$ 65.00", Image = "usa2_full.jpg" },
        new() { Title = "Summer Hat", Price = "$ 22.00", Image = "usa3_full.jpg" }
    ];

    public IReadOnlyList<UIShopItem> Items =>
        String.IsNullOrWhiteSpace(SearchText)
            ? AllItems
            : AllItems.Where(x => x.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
