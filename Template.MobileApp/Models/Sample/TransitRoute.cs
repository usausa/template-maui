namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum TrainKind
{
    Local,
    Rapid,
    SpecialRapid,
    LimitedExpress
}

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum TransitBadge
{
    Fast,
    Cheap,
    Easy,
    Eco
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record TransitStop(TimeOnly Time, string Name);

public sealed class TransitStation
{
    public required string Name { get; init; }

    public TimeOnly? Arrival { get; init; }

    public TimeOnly? Departure { get; init; }

    public required string Platform { get; init; }
}

public sealed class TransitTransfer
{
    public required int Minutes { get; init; }

    public required string FromPlatform { get; init; }

    public required string ToPlatform { get; init; }

    public required int WalkMeters { get; init; }
}

public sealed partial class TransitSegment : ObservableObject
{
    public required string Line { get; init; }

    public required string Destination { get; init; }

    public required TrainKind Kind { get; init; }

    public required Color Color { get; init; }

    public required int Minutes { get; init; }

    public required IReadOnlyList<TransitStop> Stops { get; init; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }
}

public sealed class TransitLeg
{
    public required TransitStation Station { get; init; }

    public required TransitSegment Segment { get; init; }

    public TransitTransfer? Transfer { get; init; }
}

public sealed class TransitRoute
{
    public required int Number { get; init; }

    public required TimeOnly Departure { get; init; }

    public required TimeOnly Arrival { get; init; }

    public required int Fare { get; init; }

    public required double Distance { get; init; }

    public required IReadOnlyList<TransitBadge> Badges { get; init; }

    public required IReadOnlyList<TransitLeg> Legs { get; init; }

    public required TransitStation Terminal { get; init; }

    public required StationDirection TerminalDirection { get; init; }

    public string Origin => Legs[0].Station.Name;

    public string Line => Legs[^1].Segment.Line;

    public int Minutes => (int)(Arrival - Departure).TotalMinutes;

    public int TransferCount => Legs.Count - 1;
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class TransitSample
{
    private const int ShiftMinutes = 11;

    private static readonly Color TokaidoColor = Color.FromArgb("#F57C00");

    private static readonly Color ChuoColor = Color.FromArgb("#1976D2");

    private static readonly Color SubwayColor = Color.FromArgb("#FBC02D");

    private sealed record StationSpine(string Name, string ArrivePlatform, string DepartPlatform);

    private sealed record RideSpine(
        string Line,
        string Forward,
        string Backward,
        TrainKind Kind,
        Color Color,
        int Minutes,
        IReadOnlyList<int> Offsets,
        IReadOnlyList<string> Stops);

    private sealed record RouteSpine(
        int Number,
        TimeOnly Departure,
        TimeOnly Backward,
        int Fare,
        double Distance,
        IReadOnlyList<TransitBadge> Badges,
        IReadOnlyList<StationSpine> Stations,
        IReadOnlyList<RideSpine> Rides,
        IReadOnlyList<int> Transfers,
        IReadOnlyList<int> Walks);

    private static readonly StationSpine Hozumi = new("穂積", "1番線", "2番線");

    private static readonly StationSpine Gifu = new("岐阜", "4番線", "5番線");

    private static readonly StationSpine Nagoya = new("名古屋", "2番線", "4番線");

    private static readonly StationSpine Chikusa = new("千種", "1番線", "2番線");

    private static readonly StationSpine Kozoji = new("高蔵寺", "6番線", "6番線");

    private static readonly RouteSpine[] Spines =
    [
        new(
            1,
            new TimeOnly(5, 5),
            new TimeOnly(6, 30),
            1180,
            60.3,
            [TransitBadge.Fast, TransitBadge.Cheap, TransitBadge.Easy, TransitBadge.Eco],
            [Hozumi, Nagoya, Kozoji],
            [
                new("JR東海道本線", "豊橋行", "大垣行", TrainKind.Local, TokaidoColor, 32,
                    [3, 6, 12, 16, 21, 25, 28],
                    ["西岐阜", "岐阜", "木曽川", "尾張一宮", "稲沢", "清洲", "枇杷島"]),
                new("JR中央本線", "多治見行", "名古屋行", TrainKind.Local, ChuoColor, 28,
                    [5, 8, 10, 14, 17, 21, 24, 26],
                    ["金山", "鶴舞", "千種", "大曽根", "新守山", "勝川", "春日井", "神領"])
            ],
            [3],
            [180]),
        new(
            2,
            new TimeOnly(5, 12),
            new TimeOnly(6, 38),
            1180,
            60.3,
            [TransitBadge.Cheap],
            [Hozumi, Gifu, Nagoya, Kozoji],
            [
                new("JR東海道本線", "岐阜行", "大垣行", TrainKind.Local, TokaidoColor, 8, [], []),
                new("JR東海道本線", "豊橋行", "米原行", TrainKind.SpecialRapid, TokaidoColor, 21,
                    [12], ["尾張一宮"]),
                new("JR中央本線", "中津川行", "名古屋行", TrainKind.Rapid, ChuoColor, 29,
                    [6, 11, 17, 24], ["金山", "千種", "大曽根", "春日井"])
            ],
            [6, 7],
            [60, 240]),
        new(
            3,
            new TimeOnly(5, 20),
            new TimeOnly(6, 47),
            1180,
            60.3,
            [TransitBadge.Cheap, TransitBadge.Eco],
            [Hozumi, Nagoya, Kozoji],
            [
                new("JR東海道本線", "名古屋行", "大垣行", TrainKind.Local, TokaidoColor, 39,
                    [4, 8, 15, 20, 26, 31, 35],
                    ["西岐阜", "岐阜", "木曽川", "尾張一宮", "稲沢", "清洲", "枇杷島"]),
                new("JR中央本線", "多治見行", "名古屋行", TrainKind.Local, ChuoColor, 31,
                    [5, 9, 12, 16, 19, 23, 26, 29],
                    ["金山", "鶴舞", "千種", "大曽根", "新守山", "勝川", "春日井", "神領"])
            ],
            [8],
            [180]),
        new(
            4,
            new TimeOnly(5, 30),
            new TimeOnly(6, 55),
            1510,
            58.9,
            [TransitBadge.Easy],
            [Hozumi, Nagoya, Chikusa, Kozoji],
            [
                new("JR東海道本線", "豊橋行", "大垣行", TrainKind.Local, TokaidoColor, 32,
                    [3, 6, 12, 16, 21, 25, 28],
                    ["西岐阜", "岐阜", "木曽川", "尾張一宮", "稲沢", "清洲", "枇杷島"]),
                new("地下鉄東山線", "藤が丘行", "高畑行", TrainKind.Local, SubwayColor, 8,
                    [2, 4, 6], ["伏見", "栄", "新栄町"]),
                new("JR中央本線", "中津川行", "名古屋行", TrainKind.Rapid, ChuoColor, 25,
                    [6, 14, 20], ["大曽根", "春日井", "神領"])
            ],
            [10, 9],
            [320, 150])
    ];

    public static IReadOnlyList<TransitRoute> LoadRoutes(bool reverse, int shift) =>
        [.. Spines.Select(x => Build(x, reverse, shift))];

    private static TransitRoute Build(RouteSpine spine, bool reverse, int shift)
    {
        var stations = Order(spine.Stations, reverse);
        var rides = Order(spine.Rides, reverse);
        var transfers = Order(spine.Transfers, reverse);
        var walks = Order(spine.Walks, reverse);

        var legs = new List<TransitLeg>(rides.Count);
        var depart = (reverse ? spine.Backward : spine.Departure).AddMinutes(shift * ShiftMinutes);
        var arrive = depart;

        for (var i = 0; i < rides.Count; i++)
        {
            var ride = rides[i];
            legs.Add(new TransitLeg
            {
                Station = new TransitStation
                {
                    Name = stations[i].Name,
                    Arrival = i > 0 ? arrive : null,
                    Departure = depart,
                    Platform = stations[i].DepartPlatform
                },
                Segment = new TransitSegment
                {
                    Line = ride.Line,
                    Destination = reverse ? ride.Backward : ride.Forward,
                    Kind = ride.Kind,
                    Color = ride.Color,
                    Minutes = ride.Minutes,
                    Stops = BuildStops(ride, depart, reverse)
                },
                Transfer = i > 0
                    ? new TransitTransfer
                    {
                        Minutes = transfers[i - 1],
                        FromPlatform = stations[i].ArrivePlatform,
                        ToPlatform = stations[i].DepartPlatform,
                        WalkMeters = walks[i - 1]
                    }
                    : null
            });

            arrive = depart.AddMinutes(ride.Minutes);
            if (i < rides.Count - 1)
            {
                depart = arrive.AddMinutes(transfers[i]);
            }
        }

        return new TransitRoute
        {
            Number = spine.Number,
            Departure = legs[0].Station.Departure!.Value,
            Arrival = arrive,
            Fare = spine.Fare,
            Distance = spine.Distance,
            Badges = spine.Badges,
            Legs = legs,
            Terminal = new TransitStation
            {
                Name = stations[^1].Name,
                Arrival = arrive,
                Platform = stations[^1].ArrivePlatform
            },
            TerminalDirection = reverse ? StationDirection.Up : StationDirection.Down
        };
    }

    private static IReadOnlyList<TransitStop> BuildStops(RideSpine ride, TimeOnly depart, bool reverse)
    {
        if (ride.Stops.Count == 0)
        {
            return [];
        }

        if (!reverse)
        {
            return [.. ride.Stops.Select((x, i) => new TransitStop(depart.AddMinutes(ride.Offsets[i]), x))];
        }

        // The ride runs the other way, so the stops come in reverse order at the mirrored offsets
        var count = ride.Stops.Count;
        return [.. ride.Stops.Reverse().Select((x, i) => new TransitStop(depart.AddMinutes(ride.Minutes - ride.Offsets[count - 1 - i]), x))];
    }

    private static IReadOnlyList<T> Order<T>(IReadOnlyList<T> values, bool reverse) =>
        reverse ? [.. values.Reverse()] : values;
}
