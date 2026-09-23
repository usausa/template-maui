namespace Template.MobileApp.Modules.UI;

using ClamGrid;

// 列の表示 / 順序の設定。一覧画面から編集用のコピー (GridColumnEditSession) を受け取り、Apply で結果を返す
public sealed partial class UIGridColumnViewModel : AppViewModelBase
{
    private GridColumnEditSession session = default!;

    [ObservableProperty]
    public partial GridDataView<GridColumnOption>? Rows { get; set; }

    // 行ヘッダのドラッグで順序を入れ替える (編集セッションのコレクションに反映)
    [ObservableProperty]
    public partial IGridRowMover? RowMover { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    public IObserveCommand CellValueChangedCommand { get; }

    public IObserveCommand RowMovedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIGridColumnViewModel()
    {
        CellValueChangedCommand = MakeDelegateCommand<GridCellValueEventArgs>(_ => UpdateMessage());
        RowMovedCommand = MakeDelegateCommand<GridRowMoveEventArgs>(x => Message = $"{x.OldIndex + 1} 行目を {x.NewIndex + 1} 行目へ移動");
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        session = context.Parameter.GetColumnEditSession();

        var rows = new GridDataView<GridColumnOption>(session.Columns, static x => x.Key) { SelectionMode = GridSelectionMode.None };
        Disposables.Add(rows);
        Rows = rows;
        RowMover = new GridRowMover<GridColumnOption>(session.Columns, rows);
        UpdateMessage();

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.PopAsync();

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.PopAsync(Parameters.MakeColumnOrders(session.Export()));

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void UpdateMessage() =>
        Message = $"表示 {session.Columns.Count(static x => x.IsVisible)} / {session.Columns.Count} 列。チェックで表示を切り替え、左端をドラッグして順序を変更";
}
