namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum TicketState
{
    Valid,
    Used,
    Transferred
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class TicketPass : ObservableObject
{
    public required string Tour { get; init; }

    public required string Title { get; init; }

    public required string Subtitle { get; init; }

    public required DateOnly Date { get; init; }

    public required TimeOnly Open { get; init; }

    public required TimeOnly Start { get; init; }

    public required string Venue { get; init; }

    public required string Gate { get; init; }

    public required string Seat { get; init; }

    public required string Holder { get; init; }

    public required int Companions { get; init; }

    public required string Payment { get; init; }

    public required string Number { get; init; }

    public required Color TopColor { get; init; }

    public required Color BottomColor { get; init; }

    [ObservableProperty]
    public partial TicketState State { get; set; }

    public string Code => Number.Replace("-", string.Empty, StringComparison.Ordinal);

    public Brush Artwork => new LinearGradientBrush(
        [new GradientStop(TopColor, 0f), new GradientStop(BottomColor, 1f)],
        new Point(0, 0),
        new Point(1, 1));
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed class TicketWallet
{
    public ObservableCollection<TicketPass> Passes { get; } = [];

    public int ValidCount => Passes.Count(static x => x.State == TicketState.Valid);
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class TicketSample
{
    private static readonly (string Tour, string Title, string Subtitle, string Venue, string Gate, string Seat, int Offset, int Open, string Top, string Bottom)[] Plan =
    [
        ("ARENA TOUR 2026", "STARBLAZE", "名古屋公演 ・ 2 日目", "なごやかドーム", "7 番ゲート", "3塁側 22 通路 D ブロック 14 列 208 番", 0, 17, "#E91E63", "#5E35B1"),
        ("WINTER SESSION", "WILLOW CHORUS", "大阪公演 ・ 初日", "大阪うさ城ホール", "2 番ゲート", "アリーナ A3 ブロック 6 列 42 番", 12, 18, "#00897B", "#1E88E5"),
        ("SPRING FES 2027", "WIND & FLAME", "東京公演 ・ 最終日", "東京たまごドーム", "11 番ゲート", "1塁側 14 通路 B ブロック 3 列 117 番", 40, 16, "#F57C00", "#C2185B")
    ];

    private static readonly string[] Holders = ["佐藤 太郎", "鈴木 花子", "高橋 健一"];

    public static TicketWallet LoadWallet(DateOnly today)
    {
        var wallet = new TicketWallet();
        for (var i = 0; i < Plan.Length; i++)
        {
            var (tour, title, subtitle, venue, gate, seat, offset, open, top, bottom) = Plan[i];
            wallet.Passes.Add(new TicketPass
            {
                Tour = tour,
                Title = title,
                Subtitle = subtitle,
                Date = today.AddDays(offset),
                Open = new TimeOnly(open, 0),
                Start = new TimeOnly(open + 1, 0),
                Venue = venue,
                Gate = gate,
                Seat = seat,
                Holder = Holders[i % Holders.Length],
                Companions = 2 - (i % 3),
                Payment = "クレジット払い",
                Number = CreateNumber(i),
                TopColor = Color.FromArgb(top),
                BottomColor = Color.FromArgb(bottom)
            });
        }

        return wallet;
    }

    public static TicketPass CreatePass(DateOnly today, int index)
    {
        var (tour, title, subtitle, venue, gate, seat, offset, open, top, bottom) = Plan[index % Plan.Length];
        return new TicketPass
        {
            Tour = tour,
            Title = title,
            Subtitle = subtitle,
            Date = today.AddDays(offset + 7),
            Open = new TimeOnly(open, 0),
            Start = new TimeOnly(open + 1, 0),
            Venue = venue,
            Gate = gate,
            Seat = seat,
            Holder = Holders[index % Holders.Length],
            Companions = 1,
            Payment = "コンビニ払い",
            Number = CreateNumber(index),
            TopColor = Color.FromArgb(top),
            BottomColor = Color.FromArgb(bottom)
        };
    }

    private static string CreateNumber(int index)
    {
        var seed = 4821 + (index * 137);
        return $"{seed % 10000:D4}-{(seed * 7) % 10000:D4}-{(seed * 13) % 10000:D4}";
    }
}
