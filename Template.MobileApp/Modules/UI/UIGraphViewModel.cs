namespace Template.MobileApp.Modules.UI;

using System.Text.Json;

public sealed partial class UIGraphViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string HeaderText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<GraphRow> Rows { get; private set; } = [];

    [ObservableProperty]
    public partial GraphRow? Selected { get; set; }

    // シートの内容 (閉じる途中も残す)
    [ObservableProperty]
    public partial GraphRow Detail { get; private set; } = default!;

    [ObservableProperty]
    public partial bool IsDetailOpen { get; set; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIGraphViewModel()
    {
        SubscribeSelected(OpenDetail);
        SubscribeIsDetailOpen(ClearSelection);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            await LoadAsync().ConfigureAwait(true);
        }
    }

    protected override Task OnNotifyBackAsync()
    {
        if (IsDetailOpen)
        {
            IsDetailOpen = false;
            return Task.CompletedTask;
        }

        return Navigator.ForwardAsync(ViewId.UIMenu2);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void OpenDetail(GraphRow? row)
    {
        if (row is not null)
        {
            Detail = row;
            IsDetailOpen = true;
        }
    }

    // 閉じたら選択を外す (同じ行をもう一度押しても開く)
    private void ClearSelection(bool open)
    {
        if (!open)
        {
            Selected = null;
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var data = await Task.Run(async () =>
            {
                var repository = await GraphSample.LoadRepositoryAsync().ConfigureAwait(false);
                return GraphBuilder.Build(repository.Commits, repository.Refs);
            }).ConfigureAwait(true);
            sw.Stop();

            Rows = data.Rows;
            HeaderText = $"Commits: {data.Rows.Count}    Lanes: {data.LaneCount}    Build: {sw.ElapsedMilliseconds} ms";
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            HeaderText = $"Failed to build graph: {ex.Message}";
        }
    }
}
