namespace Template.MobileApp.Models.App;

using System.Numerics;

public enum SudokuLevel
{
    Easy,
    Normal,
    Hard
}

public readonly record struct SudokuCell(int Row, int Column);

// 描く盤面 (Placed は直前に数字を入れたマス、Flash はそろった行・列・ブロックや完成で光らせるマス)
public sealed record SudokuFrame(SudokuGame Game, SudokuCell? Selected, SudokuCell? Placed, IReadOnlyList<SudokuCell> Flash);

// 保存する状態 (マスは行ごとに並べる。値の 0 は空き、メモは数字のビット。ベストタイムは難易度の順の秒で、0 は記録なし)
public sealed class SudokuSnapshot
{
    public SudokuLevel Level { get; set; }

    public IReadOnlyList<int> Solution { get; set; } = [];

    public IReadOnlyList<int> Cells { get; set; } = [];

    public IReadOnlyList<bool> Given { get; set; } = [];

    public IReadOnlyList<int> Notes { get; set; } = [];

    public int Mistakes { get; set; }

    public int HintsLeft { get; set; }

    public TimeSpan Elapsed { get; set; }

    public IReadOnlyList<int> BestSeconds { get; set; } = [];
}

// 数字のキー (答えの位置に置いていない残りの数)
public sealed partial class SudokuDigit : ObservableObject
{
    public int Value { get; }

    // キーを並べる列
    public int Column => Value - 1;

    [ObservableProperty]
    public partial int Remaining { get; set; }

    public SudokuDigit(int value)
    {
        Value = value;
    }
}

// 数独。答えが 1 つだけの問題を作り、答えと違う数字はミスとして数える (答えと同じ数字は変えられない)。完成したら難易度ごとのベストタイムを残す。
// 経過時間は数え始めた時刻と止めた時点までの時間から計算する (時刻は呼び出し側が渡す)。乱数は 0 以上 n 未満を返す関数で受け取る
public sealed class SudokuGame
{
    public const int Size = 9;

    public const int HintCount = 3;

    private const int BoxSize = 3;

    private const int CellCount = Size * Size;

    // 1 から 9 の数字のビット
    private const int AllDigits = 0b11_1111_1110;

    // マスごとの、同じ行・列・ブロックのほかのマス
    private static readonly int[][] Peers = CreatePeers();

    private readonly Func<int, int> random;

    // 行ごとに並べたマス (Row * Size + Column)。値の 0 は空き
    private readonly int[] solution = new int[CellCount];

    private readonly int[] cells = new int[CellCount];

    private readonly bool[] given = new bool[CellCount];

    // メモ (数字のビット。1 << 数字)
    private readonly int[] notes = new int[CellCount];

    // 取り消しのための変更前の状態 (1 回の操作で変わったマスをまとめる)
    private readonly Stack<CellState[]> history = new();

    // 難易度の順のベストタイム (秒。0 は記録なし)
    private readonly int[] bestSeconds = new int[Enum.GetValues<SudokuLevel>().Length];

    private TimeSpan elapsed;

    private DateTimeOffset? resumedAt;

    public SudokuLevel Level { get; private set; }

    // 答えと違う数字を入れた回数
    public int Mistakes { get; private set; }

    public int HintsLeft { get; private set; }

    public bool CanUndo => history.Count > 0;

    // 今の難易度のベストタイム (秒。0 は記録なし)
    public int BestSeconds => bestSeconds[(int)Level];

    // 完成してベストタイムを更新した
    public bool IsNewBest { get; private set; }

    // 答えになっていないマスの数
    public int RemainingCount => Enumerable.Range(0, CellCount).Count(x => cells[x] != solution[x]);

    public bool IsCompleted => RemainingCount == 0;

    // 問題のマスのほかに、数字かメモを入れている
    public bool HasProgress => Enumerable.Range(0, CellCount).Any(x => !given[x] && ((cells[x] != 0) || (notes[x] != 0)));

    public SudokuGame(Func<int, int> random)
    {
        this.random = random;
    }

    // 残すマスの数 (答えが 1 つに決まらなくなるマスは残すので、これより多くなることがある)
    private static int GetGivenCount(SudokuLevel level) => level switch
    {
        SudokuLevel.Easy => 40,
        SudokuLevel.Hard => 26,
        _ => 32
    };

    //--------------------------------------------------------------------------------
    // Game
    //--------------------------------------------------------------------------------

    // 答えを作り、消しても答えが 1 つのままのマスだけを消して問題にする
    public void NewGame(SudokuLevel level)
    {
        Array.Clear(solution);
        Fill(0);

        var puzzle = solution.ToArray();
        var count = CellCount;
        var target = GetGivenCount(level);
        foreach (var index in Shuffle(Enumerable.Range(0, CellCount).ToArray()))
        {
            if (count > target)
            {
                var value = puzzle[index];
                puzzle[index] = 0;
                if (CountSolutions(puzzle, 2) == 1)
                {
                    count--;
                }
                else
                {
                    puzzle[index] = value;
                }
            }
        }

        for (var i = 0; i < CellCount; i++)
        {
            cells[i] = puzzle[i];
            given[i] = puzzle[i] != 0;
        }

        Array.Clear(notes);
        history.Clear();
        Level = level;
        Mistakes = 0;
        HintsLeft = HintCount;
        elapsed = TimeSpan.Zero;
        resumedAt = null;
        IsNewBest = false;
    }

    // 空きのマスに、置ける数字をランダムな順で入れていく (バックトラッキング)
    private bool Fill(int index)
    {
        if (index >= CellCount)
        {
            return true;
        }

        foreach (var value in Shuffle(Enumerable.Range(1, Size).ToArray()))
        {
            if (Peers[index].All(x => solution[x] != value))
            {
                solution[index] = value;
                if (Fill(index + 1))
                {
                    return true;
                }

                solution[index] = 0;
            }
        }

        return false;
    }

    // 答えの数を上限まで数える (置ける数字のいちばん少ない空きのマスから試し、試した数字は戻す)
    private static int CountSolutions(int[] board, int limit)
    {
        var rows = new int[Size];
        var columns = new int[Size];
        var boxes = new int[Size];
        for (var i = 0; i < CellCount; i++)
        {
            if (board[i] != 0)
            {
                var bit = 1 << board[i];
                rows[i / Size] |= bit;
                columns[i % Size] |= bit;
                boxes[BoxOf(i)] |= bit;
            }
        }

        return Search(board, rows, columns, boxes, limit);
    }

    private static int Search(int[] board, int[] rows, int[] columns, int[] boxes, int limit)
    {
        var target = -1;
        var candidates = 0;
        var fewest = Size + 1;
        for (var i = 0; (i < CellCount) && (fewest > 0); i++)
        {
            if (board[i] == 0)
            {
                var mask = AllDigits & ~(rows[i / Size] | columns[i % Size] | boxes[BoxOf(i)]);
                var count = BitOperations.PopCount((uint)mask);
                if (count < fewest)
                {
                    target = i;
                    candidates = mask;
                    fewest = count;
                }
            }
        }

        if (target < 0)
        {
            return 1;
        }

        var found = 0;
        var row = target / Size;
        var column = target % Size;
        var box = BoxOf(target);
        for (var value = 1; (value <= Size) && (found < limit); value++)
        {
            var bit = 1 << value;
            if ((candidates & bit) != 0)
            {
                board[target] = value;
                rows[row] |= bit;
                columns[column] |= bit;
                boxes[box] |= bit;
                found += Search(board, rows, columns, boxes, limit - found);
                board[target] = 0;
                rows[row] &= ~bit;
                columns[column] &= ~bit;
                boxes[box] &= ~bit;
            }
        }

        return found;
    }

    //--------------------------------------------------------------------------------
    // Access
    //--------------------------------------------------------------------------------

    public int GetValue(SudokuCell cell) => cells[ToIndex(cell)];

    public bool IsGiven(SudokuCell cell) => given[ToIndex(cell)];

    // 答えと違う数字
    public bool IsWrong(SudokuCell cell)
    {
        var index = ToIndex(cell);
        return (cells[index] != 0) && (cells[index] != solution[index]);
    }

    // 同じ行・列・ブロックに同じ数字がある
    public bool HasConflict(SudokuCell cell)
    {
        var index = ToIndex(cell);
        var value = cells[index];
        var conflict = false;
        if (value != 0)
        {
            foreach (var peer in Peers[index])
            {
                conflict |= cells[peer] == value;
            }
        }

        return conflict;
    }

    public bool HasNote(SudokuCell cell, int value) => (notes[ToIndex(cell)] & (1 << value)) != 0;

    // 答えの位置に置いた数
    // 1〜9 の数字のキー
    public static IReadOnlyList<SudokuDigit> CreateDigits() =>
        [.. Enumerable.Range(1, Size).Select(static x => new SudokuDigit(x))];

    public void UpdateDigits(IEnumerable<SudokuDigit> digits)
    {
        foreach (var digit in digits)
        {
            digit.Remaining = Size - CountPlaced(digit.Value);
        }
    }

    private int CountPlaced(int value) => Enumerable.Range(0, CellCount).Count(x => (cells[x] == value) && (solution[x] == value));

    //--------------------------------------------------------------------------------
    // Input
    //--------------------------------------------------------------------------------

    // 数字を入れ、同じ行・列・ブロックのメモからその数字を消す。答えと違えばミスを数え、完成したら時間を止める
    public bool SetValue(SudokuCell cell, int value, DateTimeOffset now)
    {
        var index = ToIndex(cell);
        var changed = !IsFixed(index) && (cells[index] != value);
        if (changed)
        {
            var bit = 1 << value;
            Record([index, .. Peers[index].Where(x => (notes[x] & bit) != 0)]);
            cells[index] = value;
            notes[index] = 0;
            foreach (var peer in Peers[index])
            {
                notes[peer] &= ~bit;
            }

            if (value != solution[index])
            {
                Mistakes++;
            }
            else if (IsCompleted)
            {
                Finish(now);
            }
        }

        return changed;
    }

    // 入れた数字とメモを消す
    public bool Clear(SudokuCell cell)
    {
        var index = ToIndex(cell);
        var changed = !IsFixed(index) && ((cells[index] != 0) || (notes[index] != 0));
        if (changed)
        {
            Record([index]);
            cells[index] = 0;
            notes[index] = 0;
        }

        return changed;
    }

    // 空きのマスのメモを切り替える
    public bool ToggleNote(SudokuCell cell, int value)
    {
        var index = ToIndex(cell);
        var changed = cells[index] == 0;
        if (changed)
        {
            Record([index]);
            notes[index] ^= 1 << value;
        }

        return changed;
    }

    // 答えを入れたマスを返す。選んだマスが答えになっていなければそのマス、そうでなければ置ける数字のいちばん少ないマスに入れる
    public SudokuCell? Hint(SudokuCell? selected, DateTimeOffset now)
    {
        SudokuCell? hint = null;
        if (HintsLeft > 0)
        {
            var index = (selected is { } cell) && (cells[ToIndex(cell)] != solution[ToIndex(cell)])
                ? ToIndex(cell)
                : Enumerable.Range(0, CellCount)
                    .Where(x => cells[x] != solution[x])
                    .OrderBy(CountCandidates)
                    .FirstOrDefault(-1);
            if (index >= 0)
            {
                SetValue(ToCell(index), solution[index], now);
                HintsLeft--;
                hint = ToCell(index);
            }
        }

        return hint;
    }

    // 直前の操作を取り消し、その操作のマスを返す (ミスとヒントの回数は戻さない。完成した後は取り消さない)
    public SudokuCell? Undo()
    {
        SudokuCell? cell = null;
        if (!IsCompleted && history.TryPop(out var states))
        {
            foreach (var state in states)
            {
                cells[state.Index] = state.Value;
                notes[state.Index] = state.Notes;
            }

            cell = ToCell(states[0].Index);
        }

        return cell;
    }

    // マスを含む、すべて答えになった行・列・ブロックのマス (完成したら盤面のすべてのマス)
    public IReadOnlyList<SudokuCell> GetCompletedCells(SudokuCell cell)
    {
        var index = ToIndex(cell);
        var row = index / Size;
        var column = index % Size;
        var box = BoxOf(index);
        var units = IsCompleted
            ? Enumerable.Range(0, CellCount)
            : new[]
                {
                    Enumerable.Range(0, CellCount).Where(x => x / Size == row),
                    Enumerable.Range(0, CellCount).Where(x => x % Size == column),
                    Enumerable.Range(0, CellCount).Where(x => BoxOf(x) == box)
                }
                .Where(unit => unit.All(x => cells[x] == solution[x]))
                .SelectMany(static x => x)
                .Distinct();
        return [.. units.Select(ToCell)];
    }

    //--------------------------------------------------------------------------------
    // Time
    //--------------------------------------------------------------------------------

    public TimeSpan GetElapsed(DateTimeOffset now) =>
        elapsed + (resumedAt is { } start ? now - start : TimeSpan.Zero);

    // 数え始める (完成していれば数えない)
    public void Resume(DateTimeOffset now)
    {
        if ((resumedAt is null) && !IsCompleted)
        {
            resumedAt = now;
        }
    }

    public void Pause(DateTimeOffset now)
    {
        elapsed = GetElapsed(now);
        resumedAt = null;
    }

    // 時間を止め、経過時間 (秒は切り捨て) が短ければベストタイムにする
    private void Finish(DateTimeOffset now)
    {
        Pause(now);
        var seconds = (int)elapsed.TotalSeconds;
        var best = bestSeconds[(int)Level];
        IsNewBest = (best == 0) || (seconds < best);
        if (IsNewBest)
        {
            bestSeconds[(int)Level] = seconds;
        }
    }

    //--------------------------------------------------------------------------------
    // Snapshot
    //--------------------------------------------------------------------------------

    public SudokuSnapshot Export(DateTimeOffset now) => new()
    {
        Level = Level,
        Solution = [.. solution],
        Cells = [.. cells],
        Given = [.. given],
        Notes = [.. notes],
        Mistakes = Mistakes,
        HintsLeft = HintsLeft,
        Elapsed = GetElapsed(now),
        BestSeconds = [.. bestSeconds]
    };

    // 盤面として読めない状態は盤面を読み込まずに false (ベストタイムは読む。経過時間は止めた状態で読む)
    public bool Import(SudokuSnapshot snapshot)
    {
        for (var i = 0; i < Math.Min(bestSeconds.Length, snapshot.BestSeconds.Count); i++)
        {
            bestSeconds[i] = Math.Max(0, snapshot.BestSeconds[i]);
        }

        var valid = (snapshot.Solution.Count == CellCount) &&
                    (snapshot.Cells.Count == CellCount) &&
                    (snapshot.Given.Count == CellCount) &&
                    (snapshot.Notes.Count == CellCount) &&
                    Enumerable.Range(0, CellCount).All(x =>
                        (snapshot.Solution[x] is >= 1 and <= Size) &&
                        (snapshot.Cells[x] is >= 0 and <= Size) &&
                        (!snapshot.Given[x] || (snapshot.Cells[x] == snapshot.Solution[x])));
        if (valid)
        {
            for (var i = 0; i < CellCount; i++)
            {
                solution[i] = snapshot.Solution[i];
                cells[i] = snapshot.Cells[i];
                given[i] = snapshot.Given[i];
                notes[i] = snapshot.Notes[i] & AllDigits;
            }

            history.Clear();
            Level = snapshot.Level;
            Mistakes = snapshot.Mistakes;
            HintsLeft = snapshot.HintsLeft;
            elapsed = snapshot.Elapsed;
            resumedAt = null;
            IsNewBest = false;
        }

        return valid;
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static int ToIndex(SudokuCell cell) => (cell.Row * Size) + cell.Column;

    private static SudokuCell ToCell(int index) => new(index / Size, index % Size);

    private static int BoxOf(int index) => ((index / Size / BoxSize) * BoxSize) + ((index % Size) / BoxSize);

    private static int[][] CreatePeers() =>
    [
        .. Enumerable.Range(0, CellCount).Select(static index => Enumerable.Range(0, CellCount)
            .Where(x => (x != index) && ((x / Size == index / Size) || (x % Size == index % Size) || (BoxOf(x) == BoxOf(index))))
            .ToArray())
    ];

    // 問題のマスと、答えを入れたマスは変えない
    private bool IsFixed(int index) => given[index] || (cells[index] == solution[index]);

    // 同じ行・列・ブロックの数字から見た、置ける数字の数
    private int CountCandidates(int index) =>
        BitOperations.PopCount((uint)(AllDigits & ~Peers[index].Aggregate(0, (mask, x) => mask | (1 << cells[x]))));

    private T[] Shuffle<T>(T[] values)
    {
        for (var i = values.Length - 1; i > 0; i--)
        {
            var j = random(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }

        return values;
    }

    private void Record(IEnumerable<int> changed) =>
        history.Push([.. changed.Select(x => new CellState(x, cells[x], notes[x]))]);

    private readonly record struct CellState(int Index, int Value, int Notes);
}
