namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum ShopCategory
{
    Pc,
    Display,
    Input,
    AudioVisual,
    Storage
}

public enum ShopSort
{
    Recommended,
    PriceLow,
    PriceHigh,
    Rating
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record ShopSpec(string Name, string Value);

// Variants が空の商品は種類を選ばない
public sealed record ShopProduct(
    int Id,
    ShopCategory Category,
    string Kind,
    string Name,
    int Price,
    string Image,
    double Rating,
    int ReviewCount,
    bool IsPopular,
    string Description,
    IReadOnlyList<ShopSpec> Specs,
    string VariantName,
    IReadOnlyList<string> Variants);

// 商品とお気に入り
public sealed partial class ShopItem : ObservableObject
{
    public required ShopProduct Product { get; init; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }
}

public sealed partial class ShopCartItem : ObservableObject
{
    public required ShopProduct Product { get; init; }

    public required string Variant { get; init; }

    [ObservableProperty(NotifyAlso = [nameof(Amount)])]
    public partial int Quantity { get; set; }

    public int Amount => Product.Price * Quantity;
}

public sealed record ShopOrder(string Number, DateTime DeliveryDate, int Count, int Total);

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

// お店 (絞り込みと並べ替え・お気に入り・カート・注文)
public sealed partial class ShopStore : ObservableObject
{
    private const int MaxQuantity = 99;

    private const double DiscountRate = 0.1;

    private const int DeliveryDays = 2;

    private int orderNumber = 1290;

    public IReadOnlyList<ShopItem> Items { get; }

    public ObservableCollection<ShopCartItem> Cart { get; } = [];

    [ObservableProperty]
    public partial int CartCount { get; private set; }

    [ObservableProperty]
    public partial int Subtotal { get; private set; }

    [ObservableProperty]
    public partial int Discount { get; private set; }

    [ObservableProperty]
    public partial int Total { get; private set; }

    public ShopStore(IEnumerable<ShopProduct> products, IEnumerable<(int ProductId, string Variant, int Quantity)> cart)
    {
        Items = [.. products.Select(static x => new ShopItem { Product = x })];
        foreach (var (productId, variant, quantity) in cart)
        {
            AddToCart(Items.First(x => x.Product.Id == productId), variant, quantity);
        }
    }

    // カテゴリ (null はすべて)・名前の部分一致・価格の上限 (null は上限なし) で絞り込み、並べる
    public IReadOnlyList<ShopItem> Find(ShopCategory? category, string text, ShopSort sort, int? maxPrice)
    {
        var query = Items.Where(x =>
            ((category is null) || (x.Product.Category == category)) &&
            (String.IsNullOrWhiteSpace(text) || x.Product.Name.Contains(text.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            ((maxPrice is null) || (x.Product.Price <= maxPrice)));
        return sort switch
        {
            ShopSort.PriceLow => query.OrderBy(static x => x.Product.Price).ToArray(),
            ShopSort.PriceHigh => query.OrderByDescending(static x => x.Product.Price).ToArray(),
            ShopSort.Rating => query.OrderByDescending(static x => x.Product.Rating).ToArray(),
            _ => query.ToArray()
        };
    }

    // 同じ商品と種類の行があれば数を足す
    public void AddToCart(ShopItem item, string variant, int quantity)
    {
        var line = Cart.FirstOrDefault(x => (x.Product.Id == item.Product.Id) && (x.Variant == variant));
        if (line is null)
        {
            Cart.Add(new ShopCartItem { Product = item.Product, Variant = variant, Quantity = quantity });
        }
        else
        {
            line.Quantity = Math.Min(MaxQuantity, line.Quantity + quantity);
        }

        Update();
    }

    public void Increase(ShopCartItem item)
    {
        item.Quantity = Math.Min(MaxQuantity, item.Quantity + 1);
        Update();
    }

    // 1 より減らさない (行を消すのはスワイプ)
    public void Decrease(ShopCartItem item)
    {
        item.Quantity = Math.Max(1, item.Quantity - 1);
        Update();
    }

    public void Remove(ShopCartItem item)
    {
        Cart.Remove(item);
        Update();
    }

    // 注文を確定してカートを空にする
    public ShopOrder Checkout(DateTime today)
    {
        var order = new ShopOrder($"A{orderNumber++}", today.AddDays(DeliveryDays), CartCount, Total);
        Cart.Clear();
        Update();
        return order;
    }

    private void Update()
    {
        CartCount = Cart.Sum(static x => x.Quantity);
        Subtotal = Cart.Sum(static x => x.Amount);
        Discount = (int)Math.Round(Subtotal * DiscountRate, MidpointRounding.AwayFromZero);
        Total = Subtotal - Discount;
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// お店の見本 (商品とカート)
public static class ShopSample
{
    public static IReadOnlyList<ShopProduct> LoadProducts() =>
    [
        new(
            1, ShopCategory.Pc, "ノート PC", "スリムノート PC 14", 179800, "product_device01.jpg", 4.6, 212, true,
            "14 インチの軽いノート。重さは 1.1 kg、電池は最大 18 時間。テレワークや出張の相棒に。",
            [new("CPU", "8 コア"), new("メモリ", "16 GB"), new("ストレージ", "SSD 512 GB"), new("重さ", "1.1 kg")],
            "カラー", ["シルバー", "スペースグレー"]),
        new(
            2, ShopCategory.Display, "モニター", "4K モニター 27", 64800, "product_device02.jpg", 4.5, 134, true,
            "27 インチの 4K の IPS パネル。USB-C のケーブル 1 本で映像の入力と 65 W の給電ができます。",
            [new("解像度", "3840 × 2160"), new("パネル", "IPS"), new("入力", "USB-C / HDMI × 2"), new("給電", "65 W")],
            "スタンド", ["標準", "高さ調整"]),
        new(
            3, ShopCategory.Pc, "デスクトップ", "コンパクトデスクトップ", 129800, "product_device03.jpg", 4.3, 58, true,
            "手のひらに乗る小さな本体に 12 コアの CPU。静かで、机の上を広く使えます。",
            [new("CPU", "12 コア"), new("メモリ", "16 GB"), new("ストレージ", "SSD 1 TB"), new("大きさ", "13 × 13 × 5 cm")],
            "メモリ", ["16 GB", "32 GB"]),
        new(
            4, ShopCategory.Input, "キーボード", "メカニカルキーボード", 19800, "product_gear01.jpg", 4.7, 342, true,
            "静音のメカニカルスイッチを使ったワイヤレスキーボード。有線と Bluetooth の両対応で、最大 3 台の機器をワンタッチで切り替えられます。",
            [new("接続", "USB-C / Bluetooth 5.1"), new("配列", "日本語 91 キー"), new("電池", "最大 200 時間"), new("重さ", "780 g")],
            "スイッチ", ["赤軸", "茶軸", "青軸"]),
        new(
            5, ShopCategory.Input, "マウス", "ワイヤレスマウス", 8900, "product_gear02.jpg", 4.4, 189, false,
            "手の形に沿う静音マウス。3 台の機器をボタンで切り替えられます。",
            [new("接続", "Bluetooth / USB レシーバー"), new("解像度", "最大 4000 dpi"), new("電池", "単 3 × 1(約 18 か月)"), new("重さ", "99 g")],
            "カラー", ["ホワイト", "ブラック"]),
        new(
            6, ShopCategory.AudioVisual, "ヘッドセット", "ノイズキャンセリングヘッドセット", 14800, "product_gear03.jpg", 4.5, 276, true,
            "周りの音を抑えて会議に集中。マイクは口元の声だけを拾います。",
            [new("接続", "Bluetooth 5.3 / USB-C"), new("再生", "最大 30 時間"), new("マイク", "単一指向性"), new("重さ", "250 g")],
            "カラー", ["ネイビー", "ブラック"]),
        new(
            7, ShopCategory.AudioVisual, "Web カメラ", "4K Web カメラ", 11800, "product_gear04.jpg", 4.2, 97, false,
            "4K 30 fps と HDR で、暗い部屋でも明るく映ります。レンズを隠すシャッター付き。",
            [new("解像度", "4K 30 fps"), new("画角", "90°"), new("マイク", "ステレオ"), new("接続", "USB-C")],
            string.Empty, []),
        new(
            8, ShopCategory.Storage, "SSD", "ポータブル SSD 1TB", 16800, "product_gear05.jpg", 4.6, 158, false,
            "読み込みは最大 1,050 MB/s。手のひらに収まる大きさで、落としても壊れにくい作りです。",
            [new("容量", "1 TB"), new("速度", "最大 1,050 MB/s"), new("接続", "USB 3.2 Gen 2"), new("重さ", "58 g")],
            string.Empty, []),
        new(
            9, ShopCategory.Storage, "ドック", "USB-C ドック", 27800, "product_gear06.jpg", 4.1, 73, false,
            "ケーブル 1 本で 2 画面の出力、有線 LAN、100 W の充電をまとめてつなげます。",
            [new("映像", "HDMI × 2(4K 60 Hz)"), new("USB", "4 ポート"), new("LAN", "2.5 GbE"), new("給電", "100 W")],
            string.Empty, [])
    ];

    public static IReadOnlyList<(int ProductId, string Variant, int Quantity)> LoadCart() =>
    [
        (4, "茶軸", 1),
        (5, "ホワイト", 2),
        (6, "ネイビー", 1)
    ];
}
