namespace Template.MobileApp.Modules.Navigation.Navigate;

using Template.MobileApp;

public class NavigateInitializeViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    public NavigateInitializeViewModel(
        ApplicationState applicationState,
        IDialog dialog)
        : base(applicationState)
    {
        this.dialog = dialog;
    }

    public override async void OnNavigatedTo(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            await Navigator.PostActionAsync(() => BusyState.Using(InitializeAsync));
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected async Task InitializeAsync()
    {
        using (dialog.Loading("Initializing..."))
        {
            await Task.Delay(3000);
        }
    }
}
