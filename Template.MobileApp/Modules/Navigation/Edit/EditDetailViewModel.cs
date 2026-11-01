namespace Template.MobileApp.Modules.Navigation.Edit;

using Template.MobileApp.Services;

public sealed partial class EditDetailViewModel : AppViewModelBase, IConfirmRequestAsync
{
    // Field

    private readonly IDialog dialog;

    private readonly DataService dataService;

    private WorkEntity entity = default!;

    private string original = string.Empty;

    // Property

    [ObservableProperty]
    public partial bool IsUpdate { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public EditDetailViewModel(
        IDialog dialog,
        DataService dataService)
    {
        this.dialog = dialog;
        this.dataService = dataService;
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            IsUpdate = Equals(context.ToId, ViewId.NavigationEditDetailUpdate);
            if (IsUpdate)
            {
                entity = context.Parameter.GetValue<WorkEntity>();
                Name = entity.Name;
            }

            original = Name;
        }

        return Task.CompletedTask;
    }

    public Task<bool> CanNavigateAsync(INavigationContext context)
    {
        if (Name != original)
        {
            return dialog.ConfirmAsync("Discard changes ?").AsTask();
        }

        return Task.FromResult(true);
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

        original = Name;
        await Navigator.ForwardAsync(ViewId.NavigationEditList);
    }
}
