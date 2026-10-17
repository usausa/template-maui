namespace Template.MobileApp.Models.App;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum MinesweeperLevel
{
    Beginner,
    Intermediate
}

public enum MinesweeperState
{
    Ready,
    Playing,
    Won,
    Lost
}

public enum MinesweeperCellState
{
    Hidden,
    Opened,
    Flagged
}

public enum MinesweeperMoveType
{
    None,
    Opened,
    Flagged
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public readonly record struct MinesweeperCell(
    int Row,
    int Column);

public sealed record MinesweeperMove(
    MinesweeperMoveType Type,
    IReadOnlyList<MinesweeperCell> Revealed)
{
    public static MinesweeperMove None { get; } = new(MinesweeperMoveType.None, []);

    public static MinesweeperMove Flagged { get; } = new(MinesweeperMoveType.Flagged, []);
}

public sealed record MinesweeperFrame(
    MinesweeperGame Game,
    IReadOnlyList<MinesweeperCell> Revealed);

public sealed class MinesweeperSnapshot
{
    public IReadOnlyList<int> BestSeconds { get; set; } = [];
}

//--------------------------------------------------------------------------------
// Game
//--------------------------------------------------------------------------------

public sealed class MinesweeperGame
{
    private readonly Func<int, int> random;

    private readonly int[] bestSeconds = new int[Enum.GetValues<MinesweeperLevel>().Length];

    private bool[] mines = [];

    private int[] numbers = [];

    private MinesweeperCellState[] states = [];

    private int openedCount;

    public MinesweeperLevel Level { get; private set; }

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public int MineCount { get; private set; }

    public MinesweeperState State { get; private set; }

    public int FlagCount { get; private set; }

    public int RemainingMines => MineCount - FlagCount;

    public MinesweeperCell? Exploded { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public int BestSeconds => bestSeconds[(int)Level];

    public bool IsNewBest { get; private set; }

    private bool IsActive => State is MinesweeperState.Ready or MinesweeperState.Playing;

    public MinesweeperGame(Func<int, int> random)
    {
        this.random = random;
        NewGame(MinesweeperLevel.Beginner);
    }

    public void NewGame(MinesweeperLevel level)
    {
        (Rows, Columns, MineCount) = level == MinesweeperLevel.Intermediate ? (16, 16, 40) : (9, 9, 10);
        Level = level;
        mines = new bool[Rows * Columns];
        numbers = new int[Rows * Columns];
        states = new MinesweeperCellState[Rows * Columns];
        openedCount = 0;
        State = MinesweeperState.Ready;
        FlagCount = 0;
        Exploded = null;
        StartedAt = null;
        EndedAt = null;
        IsNewBest = false;
    }

    public bool IsMine(MinesweeperCell cell) => mines[Index(cell)];

    public int GetNumber(MinesweeperCell cell) => numbers[Index(cell)];

    public MinesweeperCellState GetState(MinesweeperCell cell) => states[Index(cell)];

    public TimeSpan GetElapsed(DateTimeOffset now) =>
        StartedAt is { } start ? (EndedAt ?? now) - start : TimeSpan.Zero;

    public MinesweeperMove Open(MinesweeperCell cell, DateTimeOffset now) =>
        GetState(cell) switch
        {
            MinesweeperCellState.Hidden => Reveal(OpenHidden(cell, now)),
            MinesweeperCellState.Opened => Reveal(Chord(cell, now)),
            _ => MinesweeperMove.None
        };

    public MinesweeperMove ToggleFlag(MinesweeperCell cell, DateTimeOffset now)
    {
        var state = GetState(cell);
        var move = MinesweeperMove.None;
        if (state == MinesweeperCellState.Opened)
        {
            move = Reveal(Chord(cell, now));
        }
        else if (IsActive)
        {
            var flagged = state == MinesweeperCellState.Hidden;
            states[Index(cell)] = flagged ? MinesweeperCellState.Flagged : MinesweeperCellState.Hidden;
            FlagCount += flagged ? 1 : -1;
            move = MinesweeperMove.Flagged;
        }

        return move;
    }

    private List<MinesweeperCell> OpenHidden(MinesweeperCell cell, DateTimeOffset now)
    {
        var opened = new List<MinesweeperCell>();
        if (IsActive && (GetState(cell) == MinesweeperCellState.Hidden))
        {
            if (State == MinesweeperState.Ready)
            {
                PlaceMines(cell);
                State = MinesweeperState.Playing;
                StartedAt = now;
            }

            OpenFrom(cell, opened, now);
        }

        return opened;
    }

    private List<MinesweeperCell> Chord(MinesweeperCell cell, DateTimeOffset now)
    {
        var opened = new List<MinesweeperCell>();
        if ((State == MinesweeperState.Playing) &&
            (GetState(cell) == MinesweeperCellState.Opened) &&
            (GetNumber(cell) > 0) &&
            (GetNeighbors(cell).Count(x => GetState(x) == MinesweeperCellState.Flagged) == GetNumber(cell)))
        {
            foreach (var neighbor in GetNeighbors(cell))
            {
                if ((State == MinesweeperState.Playing) && (GetState(neighbor) == MinesweeperCellState.Hidden))
                {
                    OpenFrom(neighbor, opened, now);
                }
            }
        }

        return opened;
    }

    private MinesweeperMove Reveal(List<MinesweeperCell> opened) =>
        opened.Count == 0
            ? MinesweeperMove.None
            : new MinesweeperMove(MinesweeperMoveType.Opened, State == MinesweeperState.Lost ? GetMines() : opened);

    private IReadOnlyList<MinesweeperCell> GetMines()
    {
        var origin = Exploded ?? new MinesweeperCell(0, 0);
        return
        [
            .. EnumerateCells()
                .Where(IsMine)
                .OrderBy(x => Math.Max(Math.Abs(x.Row - origin.Row), Math.Abs(x.Column - origin.Column)))
        ];
    }

    public MinesweeperSnapshot Export() => new()
    {
        BestSeconds = [.. bestSeconds]
    };

    public void Import(MinesweeperSnapshot snapshot)
    {
        for (var i = 0; i < Math.Min(bestSeconds.Length, snapshot.BestSeconds.Count); i++)
        {
            bestSeconds[i] = Math.Max(0, snapshot.BestSeconds[i]);
        }
    }

    private int Index(MinesweeperCell cell) => (cell.Row * Columns) + cell.Column;

    private IEnumerable<MinesweeperCell> EnumerateCells()
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                yield return new MinesweeperCell(row, column);
            }
        }
    }

    private IEnumerable<MinesweeperCell> GetNeighbors(MinesweeperCell cell)
    {
        for (var row = Math.Max(0, cell.Row - 1); row <= Math.Min(Rows - 1, cell.Row + 1); row++)
        {
            for (var column = Math.Max(0, cell.Column - 1); column <= Math.Min(Columns - 1, cell.Column + 1); column++)
            {
                if ((row != cell.Row) || (column != cell.Column))
                {
                    yield return new MinesweeperCell(row, column);
                }
            }
        }
    }

    private void PlaceMines(MinesweeperCell safe)
    {
        var candidates = EnumerateCells()
            .Where(x => (Math.Abs(x.Row - safe.Row) > 1) || (Math.Abs(x.Column - safe.Column) > 1))
            .ToList();
        for (var i = 0; i < Math.Min(MineCount, candidates.Count); i++)
        {
            var j = i + random(candidates.Count - i);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            mines[Index(candidates[i])] = true;
        }

        foreach (var cell in EnumerateCells())
        {
            numbers[Index(cell)] = GetNeighbors(cell).Count(IsMine);
        }
    }

    private void OpenFrom(MinesweeperCell start, List<MinesweeperCell> opened, DateTimeOffset now)
    {
        states[Index(start)] = MinesweeperCellState.Opened;
        opened.Add(start);
        if (IsMine(start))
        {
            Exploded = start;
            State = MinesweeperState.Lost;
            EndedAt = now;
        }
        else
        {
            openedCount++;
            var queue = new Queue<MinesweeperCell>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                if (GetNumber(cell) == 0)
                {
                    foreach (var neighbor in GetNeighbors(cell))
                    {
                        if (GetState(neighbor) == MinesweeperCellState.Hidden)
                        {
                            states[Index(neighbor)] = MinesweeperCellState.Opened;
                            openedCount++;
                            opened.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            if (openedCount == (Rows * Columns) - MineCount)
            {
                State = MinesweeperState.Won;
                EndedAt = now;
                foreach (var cell in EnumerateCells().Where(IsMine))
                {
                    states[Index(cell)] = MinesweeperCellState.Flagged;
                }

                FlagCount = MineCount;
                UpdateBest(now);
            }
        }
    }

    private void UpdateBest(DateTimeOffset now)
    {
        var seconds = (int)Math.Ceiling(GetElapsed(now).TotalSeconds);
        var best = bestSeconds[(int)Level];
        IsNewBest = (best == 0) || (seconds < best);
        if (IsNewBest)
        {
            bestSeconds[(int)Level] = seconds;
        }
    }
}
