namespace Template.MobileApp.Modules.Navigation.Edit;

using Template.MobileApp.Services;

public class EditListViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly DataService dataService;

    public ObservableCollection<WorkEntity> Items { get; } = [];

    public ICommand SelectCommand { get; }
    public ICommand DeleteCommand { get; }

    public EditListViewModel(
        ApplicationState applicationState,
        IDialog dialog,
        DataService dataService)
        : base(applicationState)
    {
        this.dialog = dialog;
        this.dataService = dataService;

        SelectCommand = MakeAsyncCommand<WorkEntity>(x =>
            Navigator.ForwardAsync(ViewId.NavigationEditDetailUpdate, new NavigationParameter().SetValue(x)));
        DeleteCommand = MakeAsyncCommand<WorkEntity>(DeleteAsync);
    }

    public override async void OnNavigatedTo(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Items.AddRange(await dataService.QueryWorkListAsync());
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(ViewId.NavigationEditDetailNew);

    private async Task DeleteAsync(WorkEntity entity)
    {
        if (!await dialog.ConfirmAsync($"Delete {entity.Name} ?"))
        {
            return;
        }

        await dataService.DeleteWorkAsync(entity.Id);

        Items.Remove(entity);
    }
}
