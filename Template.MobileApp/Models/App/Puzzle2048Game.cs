namespace Template.MobileApp.Models.App;

public enum Puzzle2048Direction
{
    Up,
    Down,
    Left,
    Right
}

// 盤面のタイル (Id で動きを追う。合体すると新しい Id になる)
public sealed record Puzzle2048Tile(int Id, int Value, int Row, int Column);

// タイルの動き (合体して消えるタイルも合体先へ動く)
public sealed record Puzzle2048Slide(int Id, int Value, int FromRow, int FromColumn, int ToRow, int ToColumn);

// 1 手の結果
public sealed class Puzzle2048Move
{
    public static Puzzle2048Move None { get; } = new([], [], null, 0);

    public IReadOnlyList<Puzzle2048Slide> Slides { get; }

    // 合体でできたタイル
    public IReadOnlyList<Puzzle2048Tile> Merged { get; }

    // 新しく置いたタイル
    public Puzzle2048Tile? Spawned { get; }

    // 加点
    public int Gained { get; }

    public bool Moved => (Merged.Count > 0) || Slides.Any(static x => (x.FromRow != x.ToRow) || (x.FromColumn != x.ToColumn));

    public Puzzle2048Move(IReadOnlyList<Puzzle2048Slide> slides, IReadOnlyList<Puzzle2048Tile> merged, Puzzle2048Tile? spawned, int gained)
    {
        Slides = slides;
        Merged = merged;
        Spawned = spawned;
        Gained = gained;
    }
}

// 描く盤面 (Move は直前の 1 手。無ければ動かさずに描く)
public sealed record Puzzle2048Frame(IReadOnlyList<Puzzle2048Tile> Tiles, Puzzle2048Move? Move);

// 保存する状態 (値は行ごとに並べる。0 は空き)
public sealed class Puzzle2048Snapshot
{
    public IReadOnlyList<int> Cells { get; set; } = [];

    public int Score { get; set; }

    public int BestScore { get; set; }

    public bool IsWon { get; set; }

    public bool KeepPlaying { get; set; }
}

// 2048。最高点は新しいゲームでも残す。乱数は 0 以上 n 未満を返す関数で受け取る
public sealed class Puzzle2048Game
{
    public const int Size = 4;

    public const int Goal = 2048;

    private readonly Func<int, int> random;

    // 行ごとに並べたマス (Row * Size + Column)
    private Puzzle2048Tile?[] cells = new Puzzle2048Tile?[Size * Size];

    private int nextId = 1;

    public int Score { get; private set; }

    // 今までの最高点 (新しいゲームでも残す)
    public int BestScore { get; private set; }

    // 2048 に届いた (一度届けば、その後も true)
    public bool IsWon { get; private set; }

    // 届いた後も続ける
    public bool KeepPlaying { get; set; }

    // 届いて、続けるかを選ぶ前 (結果を出している)
    public bool IsWinPending => IsWon && !KeepPlaying;

    // 動ける方向が無い
    public bool IsOver { get; private set; }

    public IReadOnlyList<Puzzle2048Tile> Tiles => [.. cells.OfType<Puzzle2048Tile>()];

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

    // 動けない方向と、結果を出している間は何もしない (None を返す)。動いたら新しいタイルを置く
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
                // 動く先の端から順に並べ、同じ値が続けば 1 回だけ合体する
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

            if (new Puzzle2048Move(slides, merged, null, gained).Moved)
            {
                cells = next;
                Score += gained;
                BestScore = Math.Max(BestScore, Score);
                IsWon |= merged.Any(static x => x.Value >= Goal);
                result = new Puzzle2048Move(slides, merged, Spawn(), gained);
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

    // 盤面として読めない状態は読み込まずに false
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

    // 動く先の端から並べた 1 列の位置
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
        new(tile.Id, tile.Value, tile.Row, tile.Column, row, column);

    // 空いたマスに 2 (1 割は 4) を置く
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

    // 空きがあるか、隣り合う同じ値がある
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
