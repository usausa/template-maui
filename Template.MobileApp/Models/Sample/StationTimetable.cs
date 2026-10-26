namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum StationDirection
{
    Up,
    Down
}

public enum StationDayType
{
    Weekday,
    Saturday,
    Holiday
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record StationDeparture(int Minute, TrainKind Kind, string Destination);

public sealed class StationHour
{
    public required int Hour { get; init; }

    public required IReadOnlyList<StationDeparture> Departures { get; init; }
}

public sealed class StationTimetable
{
    public required string Station { get; init; }

    public required string Line { get; init; }

    public required StationDirection Direction { get; init; }

    public required string DirectionName { get; init; }

    public required StationDayType DayType { get; init; }

    public required IReadOnlyList<StationHour> Hours { get; init; }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class StationTimetableSample
{
    private const int FirstHour = 5;

    private const int LastHour = 23;

    // Departure minutes per hour band, picked so the busy hours look dense
    private static readonly int[][] Patterns =
    [
        [8, 21, 34, 47, 56],
        [3, 11, 19, 26, 33, 40, 48, 55],
        [1, 7, 13, 18, 24, 30, 36, 41, 47, 52, 58],
        [4, 10, 16, 22, 29, 35, 42, 50, 57],
        [5, 12, 20, 31, 43, 54],
        [6, 18, 29, 41, 53],
        [9, 27, 48]
    ];

    private static readonly TrainKind[] Kinds =
    [
        TrainKind.Local,
        TrainKind.Rapid,
        TrainKind.Local,
        TrainKind.SpecialRapid,
        TrainKind.Local,
        TrainKind.Rapid,
        TrainKind.Local,
        TrainKind.LimitedExpress
    ];

    private static readonly string[] UpDestinations = ["名古屋", "金山", "大垣"];

    private static readonly string[] DownDestinations = ["多治見", "中津川", "瑞浪"];

    public static IReadOnlyList<string> LoadDirectionNames() => ["名古屋方面", "多治見・中津川方面"];

    public static StationTimetable Load(StationDirection direction, StationDayType dayType) =>
        new()
        {
            Station = "高蔵寺",
            Line = "JR中央本線",
            Direction = direction,
            DirectionName = LoadDirectionNames()[(int)direction],
            DayType = dayType,
            Hours = [.. Enumerable.Range(FirstHour, LastHour - FirstHour + 1).Select(x => BuildHour(x, direction, dayType))]
        };

    private static StationHour BuildHour(int hour, StationDirection direction, StationDayType dayType)
    {
        var pattern = Patterns[SelectPattern(hour, dayType)];
        var destinations = direction == StationDirection.Up ? UpDestinations : DownDestinations;
        var seed = hour + ((int)direction * 3) + ((int)dayType * 5);
        return new StationHour
        {
            Hour = hour,
            Departures =
            [
                .. pattern.Select((x, i) => new StationDeparture(
                    x,
                    SelectKind(hour, dayType, seed + i),
                    destinations[(seed + i) % destinations.Length]))
            ]
        };
    }

    private static int SelectPattern(int hour, StationDayType dayType)
    {
        if (hour <= 5)
        {
            return 0;
        }
        if (hour >= 23)
        {
            return 6;
        }
        if (hour >= 21)
        {
            return 5;
        }

        if (dayType == StationDayType.Weekday)
        {
            return hour switch
            {
                6 or 9 => 1,
                7 or 8 => 2,
                17 or 18 => 3,
                _ => 4
            };
        }

        return hour is >= 9 and <= 18 ? 3 : 4;
    }

    private static TrainKind SelectKind(int hour, StationDayType dayType, int index)
    {
        var kind = Kinds[index % Kinds.Length];

        if ((kind == TrainKind.LimitedExpress) && (hour is < 8 or > 20))
        {
            return TrainKind.Local;
        }

        if ((dayType == StationDayType.Weekday) && (hour is 7 or 8) && (kind == TrainKind.LimitedExpress))
        {
            return TrainKind.Rapid;
        }

        return kind;
    }
}
