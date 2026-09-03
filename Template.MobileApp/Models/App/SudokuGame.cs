namespace Template.MobileApp.Models.App;

// 数独の純モデル (UI/MAUI 非依存)。生成 (バックトラッキング)・入力・矛盾判定・完成判定を持つ。
// 盤面ロジックをモデルに閉じ込めているため、別のミニゲーム (ライフゲーム / 2048 など) を
// 追加する場合は同様の純モデルを用意し、VM が保持するモデルを差し替える
#pragma warning disable CA5394
public sealed class SudokuGame
{
    public const int Size = 9;

    private const int BoxSize = 3;

    private readonly int[][] solution = CreateBoard<int>();

    private readonly int[][] cells = CreateBoard<int>();

    private readonly bool[][] given = CreateBoard<bool>();

    private static T[][] CreateBoard<T>()
    {
        var board = new T[Size][];
        for (var i = 0; i < Size; i++)
        {
            board[i] = new T[Size];
        }

        return board;
    }

    // 完全解をバックトラッキングで生成し、指定数のマスだけ残して問題化する
    public void NewGame(int seed, int givenCount = 36)
    {
        var random = new Random(seed);

        for (var i = 0; i < Size; i++)
        {
            Array.Clear(solution[i]);
            Array.Clear(cells[i]);
            Array.Clear(given[i]);
        }

        Fill(random, 0);

        var positions = Enumerable.Range(0, Size * Size).ToArray();
        random.Shuffle(positions);
        for (var i = 0; i < givenCount; i++)
        {
            var row = positions[i] / Size;
            var col = positions[i] % Size;
            given[row][col] = true;
            cells[row][col] = solution[row][col];
        }
    }

    private bool Fill(Random random, int index)
    {
        if (index >= Size * Size)
        {
            return true;
        }

        var row = index / Size;
        var col = index % Size;

        var candidates = Enumerable.Range(1, Size).ToArray();
        random.Shuffle(candidates);
        foreach (var value in candidates)
        {
            if (CanPlace(solution, row, col, value))
            {
                solution[row][col] = value;
                if (Fill(random, index + 1))
                {
                    return true;
                }

                solution[row][col] = 0;
            }
        }

        return false;
    }

    private static bool CanPlace(int[][] board, int row, int col, int value)
    {
        for (var i = 0; i < Size; i++)
        {
            if ((board[row][i] == value) || (board[i][col] == value))
            {
                return false;
            }
        }

        var boxRow = (row / BoxSize) * BoxSize;
        var boxCol = (col / BoxSize) * BoxSize;
        for (var r = boxRow; r < boxRow + BoxSize; r++)
        {
            for (var c = boxCol; c < boxCol + BoxSize; c++)
            {
                if (board[r][c] == value)
                {
                    return false;
                }
            }
        }

        return true;
    }

    //--------------------------------------------------------------------------------
    // Access
    //--------------------------------------------------------------------------------

    public int GetValue(int row, int col) => cells[row][col];

    public bool IsGiven(int row, int col) => given[row][col];

    public bool SetValue(int row, int col, int value)
    {
        if (given[row][col])
        {
            return false;
        }

        cells[row][col] = value;
        return true;
    }

    public void ClearValue(int row, int col)
    {
        if (!given[row][col])
        {
            cells[row][col] = 0;
        }
    }

    // 同じ行/列/ボックスに重複があるか (空セルは矛盾なし)
    public bool HasConflict(int row, int col)
    {
        var value = cells[row][col];
        if (value == 0)
        {
            return false;
        }

        for (var i = 0; i < Size; i++)
        {
            if ((i != col) && (cells[row][i] == value))
            {
                return true;
            }

            if ((i != row) && (cells[i][col] == value))
            {
                return true;
            }
        }

        var boxRow = (row / BoxSize) * BoxSize;
        var boxCol = (col / BoxSize) * BoxSize;
        for (var r = boxRow; r < boxRow + BoxSize; r++)
        {
            for (var c = boxCol; c < boxCol + BoxSize; c++)
            {
                if (((r != row) || (c != col)) && (cells[r][c] == value))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsCompleted
    {
        get
        {
            for (var row = 0; row < Size; row++)
            {
                for (var col = 0; col < Size; col++)
                {
                    if ((cells[row][col] == 0) || HasConflict(row, col))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
#pragma warning restore CA5394
