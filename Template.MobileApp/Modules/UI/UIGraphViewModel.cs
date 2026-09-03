namespace Template.MobileApp.Modules.UI;

using System.Text.Json;

using Template.MobileApp.Models.Sample.Graph;

public sealed partial class UIGraphViewModel : AppViewModelBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    [ObservableProperty]
    public partial string HeaderText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<GraphRow> Rows { get; private set; } = [];

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        await LoadAsync().ConfigureAwait(true);
    }

    private async Task LoadAsync()
    {
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var data = await Task.Run(async () =>
            {
                var (commits, refs) = await LoadRepositoryAsync().ConfigureAwait(false);
                return GraphBuilder.Build(commits, refs);
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

    private static async Task<(IReadOnlyList<GraphCommit> Commits, IReadOnlyList<GraphRefData> Refs)> LoadRepositoryAsync()
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync(Path.Combine("Graph", "repository.json")).ConfigureAwait(false);
        var data = await JsonSerializer.DeserializeAsync<RepositoryData>(stream, JsonOptions).ConfigureAwait(false) ?? RepositoryData.Empty;
        return (data.Commits, data.Refs);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    private sealed class RepositoryData
    {
        public static readonly RepositoryData Empty = new();

        public List<GraphCommit> Commits { get; set; } = [];
        public List<GraphRefData> Refs { get; set; } = [];
    }
}
