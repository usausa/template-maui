namespace Template.MobileApp.Models.Sample;

using ClamGrid;

public enum OrderStatus
{
    Open,
    Processing,
    Hold,
    Completed
}

// 受注の受付経路
public enum OrderChannel
{
    Store,
    Web,
    Phone,
    Fax
}

// 目印 (期限切れ / 期限間近 / 高額 / 大口 / 最近更新)
[Flags]
public enum OrderMarks
{
    None = 0,
    Overdue = 1,
    DueSoon = 2,
    LargeAmount = 4,
    Bulk = 8,
    Recent = 16
}

// 受注一覧の 1 行。状態と確認は画面から変更されるため変更通知を持つ (グリッドが並べ替えと色を追従させる)。
// 状態・目印・ランク・受付は値のまま渡し、文言は列の Converter で決める
public sealed partial class OrderInfo : ObservableObject
{
    private const int DueSoonDays = 3;

    private const int LargeAmount = 300_000;

    private const int BulkQuantity = 20;

    private const int RecentDays = 3;

    // 列キーで値を取り出すアクセサ (XAML で宣言した GridColumn に Key で結び付く)
    public static GridValueAccessorCollection<OrderInfo> Accessors { get; } = new()
    {
        { nameof(Status), static x => x.Status },
        { nameof(OrderNo), static x => x.OrderNo },
        { nameof(Marks), static x => x.Marks },
        { nameof(Customer), static x => x.Customer },
        { nameof(Rank), static x => x.Rank },
        { nameof(Product), static x => x.Product },
        { nameof(Quantity), static x => x.Quantity },
        { nameof(Amount), static x => x.Amount },
        { nameof(DueDate), static x => x.DueDate },
        { nameof(Channel), static x => x.Channel },
        { nameof(IsChecked), static x => x.IsChecked, static (x, value) => x.IsChecked = value },
        { nameof(Staff), static x => x.Staff },
        { nameof(UpdatedAt), static x => x.UpdatedAt }
    };

    public int Id { get; }

    public string OrderNo { get; }

    public string Customer { get; }

    // 顧客ランク 1〜3
    public int Rank { get; }

    public string Product { get; }

    public int Quantity { get; }

    public int Amount { get; }

    public DateTime DueDate { get; }

    public OrderChannel Channel { get; }

    public string Staff { get; }

    public DateTime UpdatedAt { get; }

    [ObservableProperty]
    public partial OrderStatus Status { get; set; }

    [ObservableProperty]
    public partial bool IsChecked { get; set; }

    public bool IsOverdue => (Status != OrderStatus.Completed) && (DueDate < DateTime.Today);

    public bool IsDueSoon => (Status != OrderStatus.Completed) && (DueDate >= DateTime.Today) && (DueDate < DateTime.Today.AddDays(DueSoonDays));

    public bool IsLargeAmount => Amount >= LargeAmount;

    public bool IsBulk => Quantity >= BulkQuantity;

    public bool IsRecent => UpdatedAt >= DateTime.Today.AddDays(-RecentDays);

    public OrderMarks Marks =>
        (IsOverdue ? OrderMarks.Overdue : OrderMarks.None) |
        (IsDueSoon ? OrderMarks.DueSoon : OrderMarks.None) |
        (IsLargeAmount ? OrderMarks.LargeAmount : OrderMarks.None) |
        (IsBulk ? OrderMarks.Bulk : OrderMarks.None) |
        (IsRecent ? OrderMarks.Recent : OrderMarks.None);

    public OrderInfo(int id, string orderNo, string customer, int rank, string product, int quantity, int amount, DateTime dueDate, OrderChannel channel, string staff, DateTime updatedAt, OrderStatus status)
    {
        Id = id;
        OrderNo = orderNo;
        Customer = customer;
        Rank = rank;
        Product = product;
        Quantity = quantity;
        Amount = amount;
        DueDate = dueDate;
        Channel = channel;
        Staff = staff;
        UpdatedAt = updatedAt;
        Status = status;
    }

    // 未処理 → 処理中 → 完了 → 未処理 (保留は処理中へ)
    public void AdvanceStatus()
    {
        Status = Status switch
        {
            OrderStatus.Open => OrderStatus.Processing,
            OrderStatus.Processing => OrderStatus.Completed,
            OrderStatus.Hold => OrderStatus.Processing,
            _ => OrderStatus.Open
        };
        RaisePropertyChanged(nameof(Marks));
    }
}

#pragma warning disable CA5394
public static class OrderSamples
{
    private static readonly string[] Customers =
    [
        "青葉商事", "白樺工業", "海風物産", "山吹電機", "紅葉フーズ", "若草システムズ", "朝日運輸", "銀河食品", "常磐建設", "みなと印刷",
        "北斗自動車", "さくら薬品", "大地農園", "星野精機", "光陽通信"
    ];

    private static readonly (string Name, int Price)[] Products =
    [
        ("スリムノート PC 14", 179800), ("4K モニター 27", 64800), ("メカニカルキーボード", 19800), ("ワイヤレスマウス", 8900),
        ("USB-C ドック", 24800), ("Web カメラ", 12800), ("ノイズキャンセリングヘッドセット", 14800), ("外付け SSD 1TB", 15800),
        ("LAN ケーブル 5m", 980), ("モバイルバッテリー", 4980), ("ラベルプリンター", 29800), ("会議用スピーカー", 32800)
    ];

    private static readonly string[] Staffs = ["佐藤", "鈴木", "高橋", "田中", "伊藤", "渡辺", "山本", "中村"];

    // 同じ並びを再現できるよう seed 固定。顧客ランクは顧客ごとに固定
    public static OrderInfo[] Create(int count, int seed = 20260913)
    {
        var random = new Random(seed);
        var today = DateTime.Today;
        var orders = new OrderInfo[count];
        for (var i = 0; i < count; i++)
        {
            var customer = random.Next(Customers.Length);
            var (name, price) = Products[random.Next(Products.Length)];
            var quantity = random.Next(1, 30);
            var updated = today.AddDays(-random.Next(0, 30)).AddHours(random.Next(8, 20)).AddMinutes(random.Next(0, 60));
            var status = random.Next(100) switch
            {
                < 35 => OrderStatus.Open,
                < 65 => OrderStatus.Processing,
                < 75 => OrderStatus.Hold,
                _ => OrderStatus.Completed
            };
            orders[i] = new OrderInfo(
                i,
                $"SO-{today.Year}-{i + 1:D6}",
                Customers[customer],
                (customer % 3) + 1,
                name,
                quantity,
                price * quantity,
                today.AddDays(random.Next(-5, 40)),
                (OrderChannel)random.Next(4),
                Staffs[random.Next(Staffs.Length)],
                updated,
                status);
        }

        return orders;
    }
}
#pragma warning restore CA5394
