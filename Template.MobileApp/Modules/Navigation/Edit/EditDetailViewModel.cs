namespace Template.MobileApp.Modules.Navigation.Edit;

using Template.MobileApp.Models.Entity;
using Template.MobileApp.Services;

public class EditDetailViewModel : AppViewModelBase
{
    private readonly DataService dataService;

    private WorkEntity entity = default!;

    public NotificationValue<bool> IsUpdate { get; } = new();

    public NotificationValue<string> Name { get; } = new();

    public EditDetailViewModel(
        ApplicationState applicationState,
        DataService dataService)
        : base(applicationState)
    {
        this.dataService = dataService;
    }

    public override void OnNavigatingTo(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            IsUpdate.Value = Equals(context.ToId, ViewId.NavigationEditDetailUpdate);
            if (IsUpdate.Value)
            {
                entity = context.Parameter.GetValue<WorkEntity>();
                Name.Value = entity.Name;
            }
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationEditList);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction4()
    {
        if (IsUpdate.Value)
        {
            entity.Name = Name.Value;
            await dataService.UpdateWorkAsync(entity);
        }
        else
        {
            await dataService.InsertWorkAsync(Name.Value);
        }

        await Navigator.ForwardAsync(ViewId.NavigationEditList);
    }
}
