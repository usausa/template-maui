namespace Template.MobileApp.Modules.UI;

using System.Text.Json;

using Template.MobileApp.Models.Sample.Graph;

public sealed partial class UIGraph2ViewModel : AppViewModelBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [ObservableProperty]
    public partial IReadOnlyList<TimelineRow> Rows { get; private set; } = [];

    public ICommand ToggleCommand { get; }

    public UIGraph2ViewModel()
    {
        ToggleCommand = MakeDelegateCommand<TimelineRow>(static x => x.IsExpanded = !x.IsExpanded);
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (Rows.Count == 0)
        {
            await LoadAsync().ConfigureAwait(true);
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            var rows = await Task.Run(async () =>
            {
                var (commits, refs) = await LoadRepositoryAsync().ConfigureAwait(false);
                var data = GraphBuilder.Build(commits, refs);
                return data.Rows.Select(static x => new TimelineRow { Row = x }).ToList();
            }).ConfigureAwait(true);

            Rows = rows;
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to build timeline: {ex.Message}");
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
