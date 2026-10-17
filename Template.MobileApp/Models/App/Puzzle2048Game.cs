namespace Template.MobileApp.Models.App;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum Puzzle2048Direction
{
    Up,
    Down,
    Left,
    Right
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record Puzzle2048Tile(
    int Id,
    int Value,
    int Row,
    int Column);

public sealed record Puzzle2048Slide(
    int Value,
    int FromRow,
    int FromColumn,
    int ToRow,
    int ToColumn);

public sealed class Puzzle2048Move
{
    public static Puzzle2048Move None { get; } = new([], [], null, 0);

#pragma warning disable CA1819
    public Puzzle2048Slide[] Slides { get; }

    public Puzzle2048Tile[] Merged { get; }
#pragma warning restore CA1819

    public Puzzle2048Tile? Spawned { get; }

    public int Gained { get; }

    public bool Moved => (Merged.Length > 0) || Slides.Any(static x => (x.FromRow != x.ToRow) || (x.FromColumn != x.ToColumn));

    public Puzzle2048Move(Puzzle2048Slide[] slides, Puzzle2048Tile[] merged, Puzzle2048Tile? spawned, int gained)
    {
        Slides = slides;
        Merged = merged;
        Spawned = spawned;
        Gained = gained;
    }
}

#pragma warning disable CA1819
public sealed record Puzzle2048Frame(
    Puzzle2048Tile[] Tiles,
    Puzzle2048Move? Move);
#pragma warning restore CA1819

public sealed class Puzzle2048Snapshot
{
    public IReadOnlyList<int> Cells { get; set; } = [];

    public int Score { get; set; }

    public int BestScore { get; set; }

    public bool IsWon { get; set; }

    public bool KeepPlaying { get; set; }
}

//--------------------------------------------------------------------------------
// Game
//--------------------------------------------------------------------------------

public sealed class Puzzle2048Game
{
    public const int Size = 4;

    public const int Goal = 2048;

    private readonly Func<int, int> random;

    private Puzzle2048Tile?[] cells = new Puzzle2048Tile?[Size * Size];

    private int nextId = 1;

    public int Score { get; private set; }

    public int BestScore { get; private set; }

    public bool IsWon { get; private set; }

    public bool KeepPlaying { get; set; }

    public bool IsWinPending => IsWon && !KeepPlaying;

    public bool IsOver { get; private set; }

#pragma warning disable CA1819
    public Puzzle2048Tile[] Tiles => [.. cells.OfType<Puzzle2048Tile>()];
#pragma warning restore CA1819

    public Puzzle2048Game(Func<int, int> random)
    {
        this.random = random;
    }

    public void NewGame()
    {
        cells = new Puzzle2048Tile?[Size * Size];
        Score = 0;
        IsWon = false;
        KeepPlaying = false;
        Spawn();
        Spawn();
        IsOver = false;
    }

    public Puzzle2048Move Move(Puzzle2048Direction direction)
    {
        var result = Puzzle2048Move.None;
        if (!IsOver && !IsWinPending)
        {
            var next = new Puzzle2048Tile?[Size * Size];
            var slides = new List<Puzzle2048Slide>();
            var merged = new List<Puzzle2048Tile>();
            var gained = 0;
            for (var line = 0; line < Size; line++)
            {
                var positions = GetLine(direction, line);
                var tiles = positions.Select(x => cells[Index(x.Row, x.Column)]).OfType<Puzzle2048Tile>().ToList();
                var target = 0;
                var index = 0;
                while (index < tiles.Count)
                {
                    var tile = tiles[index];
                    var (row, column) = positions[target];
                    if ((index + 1 < tiles.Count) && (tiles[index + 1].Value == tile.Value))
                    {
                        var joined = new Puzzle2048Tile(nextId++, tile.Value * 2, row, column);
                        slides.Add(ToSlide(tile, row, column));
                        slides.Add(ToSlide(tiles[index + 1], row, column));
                        merged.Add(joined);
                        next[Index(row, column)] = joined;
                        gained += joined.Value;
                        index += 2;
                    }
                    else
                    {
                        slides.Add(ToSlide(tile, row, column));
                        next[Index(row, column)] = tile with { Row = row, Column = column };
                        index++;
                    }

                    target++;
                }
            }

            var moved = new Puzzle2048Move([.. slides], [.. merged], null, gained);
            if (moved.Moved)
            {
                cells = next;
                Score += gained;
                BestScore = Math.Max(BestScore, Score);
                IsWon |= merged.Any(static x => x.Value >= Goal);
                result = new Puzzle2048Move(moved.Slides, moved.Merged, Spawn(), gained);
                IsOver = !CanMove();
            }
        }

        return result;
    }

    public Puzzle2048Snapshot Export() => new()
    {
        Cells = [.. cells.Select(static x => x?.Value ?? 0)],
        Score = Score,
        BestScore = BestScore,
        IsWon = IsWon,
        KeepPlaying = KeepPlaying
    };

    public bool Import(Puzzle2048Snapshot snapshot)
    {
        var valid = (snapshot.Cells.Count == Size * Size) && snapshot.Cells.Any(static x => x > 0);
        if (valid)
        {
            cells = new Puzzle2048Tile?[Size * Size];
            for (var i = 0; i < snapshot.Cells.Count; i++)
            {
                if (snapshot.Cells[i] > 0)
                {
                    cells[i] = new Puzzle2048Tile(nextId++, snapshot.Cells[i], i / Size, i % Size);
                }
            }

            Score = snapshot.Score;
            BestScore = Math.Max(snapshot.BestScore, snapshot.Score);
            IsWon = snapshot.IsWon;
            KeepPlaying = snapshot.KeepPlaying;
            IsOver = !CanMove();
        }

        return valid;
    }

    private static (int Row, int Column)[] GetLine(Puzzle2048Direction direction, int line)
    {
        var positions = new (int Row, int Column)[Size];
        for (var i = 0; i < Size; i++)
        {
            positions[i] = direction switch
            {
                Puzzle2048Direction.Left => (line, i),
                Puzzle2048Direction.Right => (line, Size - 1 - i),
                Puzzle2048Direction.Up => (i, line),
                _ => (Size - 1 - i, line)
            };
        }

        return positions;
    }

    private static int Index(int row, int column) => (row * Size) + column;

    private static Puzzle2048Slide ToSlide(Puzzle2048Tile tile, int row, int column) =>
        new(tile.Value, tile.Row, tile.Column, row, column);

    private Puzzle2048Tile? Spawn()
    {
        var empties = new List<(int Row, int Column)>();
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                if (cells[Index(row, column)] is null)
                {
                    empties.Add((row, column));
                }
            }
        }

        Puzzle2048Tile? tile = null;
        if (empties.Count > 0)
        {
            var (row, column) = empties[random(empties.Count)];
            tile = new Puzzle2048Tile(nextId++, random(10) == 0 ? 4 : 2, row, column);
            cells[Index(row, column)] = tile;
        }

        return tile;
    }

    private bool CanMove()
    {
        var movable = false;
        for (var row = 0; (row < Size) && !movable; row++)
        {
            for (var column = 0; (column < Size) && !movable; column++)
            {
                var value = cells[Index(row, column)]?.Value ?? 0;
                movable = (value == 0) ||
                          ((column + 1 < Size) && (cells[Index(row, column + 1)]?.Value == value)) ||
                          ((row + 1 < Size) && (cells[Index(row + 1, column)]?.Value == value));
            }
        }

        return movable;
    }
}
