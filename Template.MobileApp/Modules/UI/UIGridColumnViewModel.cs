namespace Template.MobileApp.Modules.UI;

using ClamGrid;

public static class ColumnOptionAccessors
{
    public static GridValueAccessorCollection<GridColumnOption> Option { get; } = new()
    {
        { nameof(GridColumnOption.IsVisible), static x => x.IsVisible, static (x, value) => x.IsVisible = value },
        { nameof(GridColumnOption.Header), static x => x.Header }
    };
}

public sealed partial class UIGridColumnViewModel : AppViewModelBase
{
    // Field

    private GridColumnEditSession session = default!;

    // Property

    [ObservableProperty]
    public partial GridDataView<GridColumnOption>? Rows { get; set; }

    [ObservableProperty]
    public partial IGridRowMover? RowMover { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    // Command

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
        session = context.Parameter.GetModel<GridColumnEditSession>();

        var rows = new GridDataView<GridColumnOption>(session.Columns, static x => x.Key) { SelectionMode = GridSelectionMode.None };
        Disposables.Add(rows);
        Rows = rows;
        RowMover = new GridRowMover<GridColumnOption>(session.Columns, rows);
        UpdateMessage();

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.PopAsync();

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.PopAsync(Parameters.MakeModel(session.Export()));

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void UpdateMessage() =>
        Message = $"表示 {session.Columns.Count(static x => x.IsVisible)} / {session.Columns.Count} 列";
}
