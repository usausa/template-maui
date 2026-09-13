namespace Template.MobileApp.Modules.App;

using Template.MobileApp.Models.App;

public sealed partial class SudokuCellViewModel : ObservableObject
{
    public int Row { get; }

    public int Col { get; }

    // 3x3 ブロックの区切りを太くするための余白
    // 盤面 Grid 上の位置 (3x3 ブロックの境界に太線用のスペーサ行・列を挟むため 1 つずつずれる)
    public int GridRow { get; }

    public int GridColumn { get; }

    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsGiven { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial bool IsConflict { get; set; }

    public SudokuCellViewModel(int row, int col)
    {
        Row = row;
        Col = col;
        GridRow = row + (row / 3);
        GridColumn = col + (col / 3);
    }
}

#pragma warning disable CA5394
public sealed partial class AppGameViewModel : AppViewModelBase
{
    private readonly Random random = new();

    // 盤面ロジックは純モデル (差し替え可能)。VM は表示状態の同期のみを担う
    private readonly SudokuGame game = new();

    private SudokuCellViewModel? selected;

    public IReadOnlyList<SudokuCellViewModel> Cells { get; }

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    public IObserveCommand SelectCommand { get; }

    public IObserveCommand NumberCommand { get; }

    public IObserveCommand EraseCommand { get; }

    public AppGameViewModel()
    {
        var cells = new List<SudokuCellViewModel>(SudokuGame.Size * SudokuGame.Size);
        for (var row = 0; row < SudokuGame.Size; row++)
        {
            for (var col = 0; col < SudokuGame.Size; col++)
            {
                cells.Add(new SudokuCellViewModel(row, col));
            }
        }

        Cells = cells;

        SelectCommand = MakeDelegateCommand<SudokuCellViewModel>(Select);
        NumberCommand = MakeDelegateCommand<string>(InputNumber);
        EraseCommand = MakeDelegateCommand(Erase);

        NewGame();
    }

    private void NewGame()
    {
        game.NewGame(random.Next());
        selected = null;
        IsCompleted = false;
        RefreshAll();
    }

    // 空きまたは誤った入力のマスからランダムに 1 つ選び正解を入れる (F3 を押すたびに 1 マス)
    private void AutoStep()
    {
        if (IsCompleted)
        {
            return;
        }

        var candidates = Cells
            .Where(x => !game.IsGiven(x.Row, x.Col) && (game.GetValue(x.Row, x.Col) != game.GetSolution(x.Row, x.Col)))
            .ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        var cell = candidates[random.Next(candidates.Count)];
        game.SetValue(cell.Row, cell.Col, game.GetSolution(cell.Row, cell.Col));
        selected = cell;
        RefreshAll();
        IsCompleted = game.IsCompleted;
    }

    private void Select(SudokuCellViewModel cell)
    {
        selected?.IsSelected = false;

        selected = cell;
        cell.IsSelected = true;
    }

    private void InputNumber(string number)
    {
        if ((selected is null) || IsCompleted)
        {
            return;
        }

        if (game.SetValue(selected.Row, selected.Col, int.Parse(number, CultureInfo.InvariantCulture)))
        {
            RefreshAll();
            IsCompleted = game.IsCompleted;
        }
    }

    private void Erase()
    {
        if ((selected is null) || IsCompleted)
        {
            return;
        }

        game.ClearValue(selected.Row, selected.Col);
        RefreshAll();
    }

    private void RefreshAll()
    {
        foreach (var cell in Cells)
        {
            var value = game.GetValue(cell.Row, cell.Col);
            cell.Text = value == 0 ? string.Empty : value.ToString(CultureInfo.InvariantCulture);
            cell.IsGiven = game.IsGiven(cell.Row, cell.Col);
            cell.IsConflict = game.HasConflict(cell.Row, cell.Col);
            cell.IsSelected = ReferenceEquals(cell, selected);
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.AppMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction3()
    {
        AutoStep();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        NewGame();
        return Task.CompletedTask;
    }
}
#pragma warning restore CA5394
