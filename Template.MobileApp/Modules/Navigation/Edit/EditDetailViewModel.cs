namespace Template.MobileApp.Modules.Navigation.Edit;

using Template.MobileApp.Services;

public sealed partial class EditDetailViewModel : AppViewModelBase
{
    private readonly DataService dataService;

    private WorkEntity entity = default!;

    [ObservableProperty]
    public partial bool IsUpdate { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = default!;

    public EditDetailViewModel(
        DataService dataService)
    {
        this.dataService = dataService;
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            IsUpdate = Equals(context.ToId, ViewId.NavigationEditDetailUpdate);
            if (IsUpdate)
            {
                entity = context.Parameter.GetValue<WorkEntity>();
                Name = entity.Name;
            }
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationEditList);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction4()
    {
        if (IsUpdate)
        {
            entity.Name = Name;
            await dataService.UpdateWorkAsync(entity);
        }
        else
        {
            await dataService.InsertWorkAsync(Name);
        }

        await Navigator.ForwardAsync(ViewId.NavigationEditList);
    }
}
