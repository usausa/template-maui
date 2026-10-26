namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum SeatKind
{
    None,
    Standard,
    Premium,
    Wheelchair
}

public enum SeatStatus
{
    Available,
    Sold,
    Selected
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class SeatPlace : ObservableObject
{
    public required int Row { get; init; }

    public required int Column { get; init; }

    public required SeatKind Kind { get; init; }

    [ObservableProperty]
    public partial SeatStatus Status { get; set; }

    public string Name => $"{(char)('A' + Row)}-{Column + 1}";
}

public sealed class SeatScreening
{
    public required string Title { get; init; }

    public required string Screen { get; init; }

    public required DateTime Start { get; init; }

    public required DateTime End { get; init; }

    public required bool Subtitled { get; init; }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed class SeatMap
{
    private readonly SeatPlace?[] places;

    public int Rows { get; }

    public int Columns { get; }

    public IReadOnlyList<int> Aisles { get; }

    public int Limit { get; }

    public SeatMap(int rows, int columns, IReadOnlyList<int> aisles, IEnumerable<SeatPlace> seats, int limit)
    {
        Rows = rows;
        Columns = columns;
        Aisles = aisles;
        Limit = limit;
        places = new SeatPlace?[rows * columns];
        foreach (var seat in seats)
        {
            places[(seat.Row * columns) + seat.Column] = seat;
        }
    }

    public SeatPlace? Find(int row, int column) =>
        (row >= 0) && (row < Rows) && (column >= 0) && (column < Columns) ? places[(row * Columns) + column] : null;

    public IEnumerable<SeatPlace> Enumerate() => places.OfType<SeatPlace>();

    public IReadOnlyList<SeatPlace> SelectedPlaces() =>
        [.. Enumerate().Where(static x => x.Status == SeatStatus.Selected)];

    public bool Toggle(SeatPlace place)
    {
        if (place.Status == SeatStatus.Selected)
        {
            place.Status = SeatStatus.Available;
            return true;
        }

        if ((place.Status != SeatStatus.Available) || (SelectedPlaces().Count >= Limit))
        {
            return false;
        }

        place.Status = SeatStatus.Selected;
        return true;
    }

    public void Confirm()
    {
        foreach (var place in SelectedPlaces())
        {
            place.Status = SeatStatus.Sold;
        }
    }

    public static int PriceOf(SeatKind kind) => kind switch
    {
        SeatKind.Premium => 2500,
        SeatKind.Wheelchair => 1000,
        _ => 1800
    };
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class SeatSample
{
    private const int RowCount = 16;

    private const int ColumnCount = 16;

    private const int SelectionLimit = 6;

    private const int PremiumFirstRow = 7;

    private const int PremiumLastRow = 8;

    private static readonly int[] AisleColumns = [3, 13];

    private static readonly (string Title, string Screen, int Hour, int Minutes, bool Subtitled)[] Showings =
    [
        ("ネビュラ・フロンティア 4K リマスター", "SCREEN 7", 19, 170, true),
        ("サイレント・ホライズン", "SCREEN 3", 13, 128, false),
        ("劇場版 鋼鉄の咆哮", "SCREEN 1", 10, 96, true),
        ("星降る森の祭り 完全版", "SCREEN 7", 22, 145, true)
    ];

    public static IReadOnlyList<string> LoadShowingNames() =>
        [.. Showings.Select(static x => $"{x.Hour:D2}:10")];

    public static SeatScreening LoadScreening(int index, DateTime today)
    {
        var (title, screen, hour, minutes, subtitled) = Showings[index % Showings.Length];
        var start = today.Date.AddHours(hour).AddMinutes(10);
        return new SeatScreening
        {
            Title = title,
            Screen = screen,
            Start = start,
            End = start.AddMinutes(minutes),
            Subtitled = subtitled
        };
    }

    public static SeatMap LoadMap(int index)
    {
        var seats = new List<SeatPlace>(RowCount * ColumnCount);
        for (var row = 0; row < RowCount; row++)
        {
            for (var column = 0; column < ColumnCount; column++)
            {
                seats.Add(new SeatPlace
                {
                    Row = row,
                    Column = column,
                    Kind = KindOf(row, column),
                    Status = IsSold(row, column, index) ? SeatStatus.Sold : SeatStatus.Available
                });
            }
        }

        return new SeatMap(RowCount, ColumnCount, AisleColumns, seats, SelectionLimit);
    }

    private static SeatKind KindOf(int row, int column)
    {
        if ((row == RowCount - 1) && ((column == 0) || (column == ColumnCount - 1)))
        {
            return SeatKind.Wheelchair;
        }

        return row is >= PremiumFirstRow and <= PremiumLastRow ? SeatKind.Premium : SeatKind.Standard;
    }

    // Deterministic scatter so each showing looks different but stays stable
    private static bool IsSold(int row, int column, int index)
    {
        var hash = ((row + 1) * 73) ^ ((column + 1) * 151) ^ ((index + 1) * 331);
        hash = (hash * 2654435761u.GetHashCode()) & 0x7FFFFFFF;
        var density = row < 4 ? 46 : row < 10 ? 30 : 18;
        return (hash % 100) < density;
    }
}
