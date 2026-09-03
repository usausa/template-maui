namespace Template.MobileApp.Modules.App;

using Template.MobileApp.Models.App;

public sealed partial class SudokuCellViewModel : ObservableObject
{
    public int Row { get; }

    public int Col { get; }

    // 3x3 ブロックの区切りを太くするための余白
    public Thickness Margin { get; }

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
        Margin = new Thickness(
            0,
            0,
            col is 2 or 5 ? 4 : 1,
            row is 2 or 5 ? 4 : 1);
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

    public IObserveCommand NewGameCommand { get; }

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
        NewGameCommand = MakeDelegateCommand(NewGame);

        NewGame();
    }

    private void NewGame()
    {
        game.NewGame(random.Next());
        selected = null;
        IsCompleted = false;
        RefreshAll();
    }

    private void Select(SudokuCellViewModel cell)
    {
        if (selected is not null)
        {
            selected.IsSelected = false;
        }

        selected = cell;
        cell.IsSelected = true;
    }

    private void InputNumber(string number)
    {
        if ((selected is null) || IsCompleted)
        {
            return;
        }

        if (game.SetValue(selected.Row, selected.Col, int.Parse(number, System.Globalization.CultureInfo.InvariantCulture)))
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
            cell.Text = value == 0 ? string.Empty : value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            cell.IsGiven = game.IsGiven(cell.Row, cell.Col);
            cell.IsConflict = game.HasConflict(cell.Row, cell.Col);
            cell.IsSelected = ReferenceEquals(cell, selected);
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.AppMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
#pragma warning restore CA5394
