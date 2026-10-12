namespace Template.MobileApp.Modules.UI;

using System.Text.Json;

public sealed partial class UIGraph2ViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial IReadOnlyList<Selectable<GraphRow>> Rows { get; private set; } = [];

    public ICommand ToggleCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIGraph2ViewModel()
    {
        ToggleCommand = MakeDelegateCommand<Selectable<GraphRow>>(static x => x.IsSelected = !x.IsSelected);
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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private async Task LoadAsync()
    {
        try
        {
            var rows = await Task.Run(async () =>
            {
                var repository = await GraphSample.LoadRepositoryAsync().ConfigureAwait(false);
                var data = GraphBuilder.Build(repository.Commits, repository.Refs);
                return data.Rows.Select(static x => new Selectable<GraphRow>(x)).ToList();
            }).ConfigureAwait(true);

            Rows = rows;
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to build timeline: {ex.Message}");
        }
    }
}
