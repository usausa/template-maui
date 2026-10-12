namespace Template.MobileApp.Modules.UI;

public sealed record UIShopCategoryOption(ShopCategory? Category, string Name);

public sealed record UIShopSortOption(ShopSort Sort, string Name);

public sealed record UIShopPriceOption(int? MaxPrice, string Name);

// 人気の商品のカルーセルの項目 (中央の項目を強調する)
public sealed partial class UIShopPopularItem : ObservableObject
{
    public required ShopItem Item { get; init; }

    [ObservableProperty]
    public partial bool IsCurrent { get; set; }
}

public sealed partial class UIShopViewModel : AppViewModelBase
{
    private static readonly UIShopCategoryOption[] CategoryOptions =
    [
        new(null, "すべて"),
        new(ShopCategory.Pc, "PC"),
        new(ShopCategory.Display, "ディスプレイ"),
        new(ShopCategory.Input, "入力機器"),
        new(ShopCategory.AudioVisual, "オーディオ・映像"),
        new(ShopCategory.Storage, "ストレージ・拡張")
    ];

    private static readonly UIShopSortOption[] SortOptions =
    [
        new(ShopSort.Recommended, "おすすめ"),
        new(ShopSort.PriceLow, "安い順"),
        new(ShopSort.PriceHigh, "高い順"),
        new(ShopSort.Rating, "評価の高い順")
    ];

    private static readonly UIShopPriceOption[] PriceOptions =
    [
        new(null, "指定なし"),
        new(10000, "〜 1 万円"),
        new(30000, "〜 3 万円"),
        new(100000, "〜 10 万円")
    ];

    [Scope]
    [ObservableProperty]
    public partial UIShopContext Context { get; set; } = default!;

    public string Greeting { get; } = "こんにちは、うさうささん";

    public string SubGreeting { get; } = "デスク周りをアップグレード";

    public IReadOnlyList<UIShopCategoryOption> Categories { get; } = CategoryOptions;

    public IReadOnlyList<UIShopSortOption> Sorts { get; } = SortOptions;

    public IReadOnlyList<UIShopPriceOption> Prices { get; } = PriceOptions;

    [ObservableProperty]
    public partial UIShopCategoryOption SelectedCategory { get; set; } = CategoryOptions[0];

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty(NotifyAlso = [nameof(IsFiltered)])]
    public partial UIShopSortOption SelectedSort { get; set; } = SortOptions[0];

    [ObservableProperty(NotifyAlso = [nameof(IsFiltered)])]
    public partial UIShopPriceOption SelectedPrice { get; set; } = PriceOptions[0];

    // 並び順か価格を指定している (絞り込みのボタンに印)
    public bool IsFiltered => (SelectedSort != SortOptions[0]) || (SelectedPrice != PriceOptions[0]);

    [ObservableProperty]
    public partial bool IsFilterOpen { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<UIShopPopularItem> Popular { get; set; } = [];

    public ObservableCollection<ShopItem> Items { get; } = [];

    public IObserveCommand ItemCommand { get; }

    public IObserveCommand CurrentChangedCommand { get; }

    public IObserveCommand FavoriteCommand { get; }

    public IObserveCommand CartCommand { get; }

    public IObserveCommand FilterCommand { get; }

    public IObserveCommand ResetFilterCommand { get; }

    public IObserveCommand CloseFilterCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIShopViewModel()
    {
        SubscribeSelectedCategory(_ => Filter());
        SubscribeSearchText(_ => Filter());
        SubscribeSelectedSort(_ => Filter());
        SubscribeSelectedPrice(_ => Filter());

        ItemCommand = MakeAsyncCommand<ShopItem>(OpenAsync);
        CurrentChangedCommand = MakeDelegateCommand<UIShopPopularItem>(x =>
        {
            foreach (var item in Popular)
            {
                item.IsCurrent = item == x;
            }
        });
        FavoriteCommand = MakeDelegateCommand<ShopItem>(static x => x.IsFavorite = !x.IsFavorite);
        CartCommand = MakeAsyncCommand(() => Navigator.PushAsync(ViewId.UICart));

        FilterCommand = MakeDelegateCommand(() => IsFilterOpen = true);
        ResetFilterCommand = MakeDelegateCommand(() =>
        {
            SelectedSort = SortOptions[0];
            SelectedPrice = PriceOptions[0];
        });
        CloseFilterCommand = MakeDelegateCommand(() => IsFilterOpen = false);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    // 商品とカートから戻ったときは、お気に入りとカートの数を文脈が反映している
    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Popular = Context.Store.Items
                .Where(static x => x.Product.IsPopular)
                .Select(static x => new UIShopPopularItem { Item = x })
                .ToArray();
            Popular[0].IsCurrent = true;
            Filter();
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private Task OpenAsync(ShopItem item)
    {
        Context.Selected = item;
        return Navigator.PushAsync(ViewId.UIItem);
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    // 一覧を入れ替えたり途中で空にしたりすると見出しまで作り直されて動きが出直すので、先に足して並べ、残りを外す
    private void Filter()
    {
        var items = Context.Store.Find(SelectedCategory.Category, SearchText, SelectedSort.Sort, SelectedPrice.MaxPrice);
        for (var i = 0; i < items.Count; i++)
        {
            var index = Items.IndexOf(items[i]);
            if (index < 0)
            {
                Items.Insert(i, items[i]);
            }
            else if (index != i)
            {
                Items.Move(index, i);
            }
        }

        while (Items.Count > items.Count)
        {
            Items.RemoveAt(Items.Count - 1);
        }
    }
}
