namespace Template.MobileApp.Modules.Navigation.Navigate;

public sealed partial class NavigateInitializeViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    [ObservableProperty]
    public partial bool Initialized { get; set; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public NavigateInitializeViewModel(
        IDialog dialog)
    {
        this.dialog = dialog;
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            await Navigator.PostActionAsync(InitializeAsync);
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private Task InitializeAsync()
    {
        return BusyState.UsingAsync(async () =>
        {
            using (dialog.Loading("Initializing..."))
            {
                await Task.Delay(3000);
            }

            Initialized = true;
        });
    }
}
