namespace Template.MobileApp.Modules.UI;

using ClamGrid;

using Template.MobileApp.Models.Sample;

public sealed partial class UIGridViewModel : AppViewModelBase
{
    private const int RowCount = 2000;

    private static readonly GridSortOrder[] DefaultSortOrders = [new(nameof(OrderInfo.DueDate)), new(nameof(OrderInfo.OrderNo))];

    private readonly IDialog dialog;

    public GridDataView<OrderInfo> Rows { get; }

    [ObservableProperty]
    public partial IReadOnlyList<GridColumnOrder>? ColumnOrders { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<GridSortOrder>? SortOrders { get; set; }

    [ObservableProperty]
    public partial int OpenCount { get; set; }

    [ObservableProperty]
    public partial int SelectedCount { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    public IObserveCommand SelectAllCommand { get; }

    public IObserveCommand ColumnEditCommand { get; }

    public IObserveCommand CellValueChangedCommand { get; }

    public IObserveCommand CommitCommand { get; }

    public IObserveCommand AdvanceCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIGridViewModel(IDialog dialog)
    {
        this.dialog = dialog;

        Rows = new GridDataView<OrderInfo>(Array.Empty<OrderInfo>(), static x => x.Id);
        Rows.RegisterSort(nameof(OrderInfo.Status), static x => x.Status);
        Rows.RegisterSort(nameof(OrderInfo.OrderNo), static x => x.Id);
        Rows.RegisterSort(nameof(OrderInfo.Customer), static x => x.Customer, StringComparer.CurrentCulture);
        Rows.RegisterSort(nameof(OrderInfo.Rank), static x => x.Rank);
        Rows.RegisterSort(nameof(OrderInfo.Product), static x => x.Product, StringComparer.CurrentCulture);
        Rows.RegisterSort(nameof(OrderInfo.Quantity), static x => x.Quantity);
        Rows.RegisterSort(nameof(OrderInfo.Amount), static x => x.Amount);
        Rows.RegisterSort(nameof(OrderInfo.DueDate), static x => x.DueDate);
        Rows.RegisterSort(nameof(OrderInfo.Channel), static x => x.Channel);
        Rows.RegisterSort(nameof(OrderInfo.Staff), static x => x.Staff, StringComparer.CurrentCulture);
        Rows.RegisterSort(nameof(OrderInfo.UpdatedAt), static x => x.UpdatedAt);

        SortOrders = DefaultSortOrders;

        SelectAllCommand = MakeDelegateCommand<bool>(x => Rows.UpdateSelection(row => x && (row.Status == OrderStatus.Open)));
        ColumnEditCommand = MakeAsyncCommand<GridColumnConfigurationEventArgs>(x =>
            Navigator.PushAsync(ViewId.UIGridColumn, Parameters.MakeModel(x.CreateEditSession())));
        CellValueChangedCommand = MakeDelegateCommand<GridCellValueEventArgs>(x =>
            Message = $"確認: {((OrderInfo)x.Item).OrderNo} = {(x.NewValue ? "済" : "未")}");
        CommitCommand = MakeAsyncCommand(CommitAsync, () => SelectedCount > 0);
        AdvanceCommand = MakeDelegateCommand(Advance, () => SelectedCount > 0);

        Disposables.Add(Rows.PropertyChangedAsObservable()
            .Where(x => x.PropertyName is nameof(Rows.SelectedCount) or nameof(Rows.Count))
            .Subscribe(_ => SelectedCount = Rows.SelectedCount));
        Disposables.Add(Rows);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Load();
        }
        else if (context.Parameter.TryGetModel<GridColumnOrder[]>(out var orders))
        {
            ColumnOrders = orders;
            Message = $"列設定を反映: 表示 {orders.Count(static x => x.IsVisible)} / {orders.Length} 列";
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Rows.ClearSelection();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        ColumnOrders = null;
        SortOrders = DefaultSortOrders;
        Message = "列設定とソートを既定に戻しました";
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        Load();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Load()
    {
        Rows.SetSource(OrderSample.LoadOrders(RowCount));
        UpdateCounts();
        Message = string.Empty;
    }

    private async Task CommitAsync()
    {
        var selected = Rows.SelectedItems.Cast<OrderInfo>().ToList();
        var amount = selected.Sum(static x => (long)x.Amount);
        await dialog.InformationAsync(String.Format(CultureInfo.CurrentCulture, "{0} 件を確定しました。\n合計 ¥{1:N0}", selected.Count, amount));
        Message = $"確定: {selected.Count} 件 / {String.Join(", ", selected.Take(3).Select(static x => x.OrderNo))}{(selected.Count > 3 ? " …" : string.Empty)}";
    }

    private void Advance()
    {
        var selected = Rows.SelectedItems.Cast<OrderInfo>().ToHashSet();
        Rows.Suspend();
        foreach (var row in selected)
        {
            row.AdvanceStatus();
        }

        Rows.Resume();
        Rows.UpdateSelection(selected.Contains);

        UpdateCounts();
        Message = $"状態更新: {selected.Count} 件";
    }

    private void UpdateCounts()
    {
        var open = 0;
        foreach (var row in Rows)
        {
            if (row.Status == OrderStatus.Open)
            {
                open++;
            }
        }

        OpenCount = open;
    }
}
