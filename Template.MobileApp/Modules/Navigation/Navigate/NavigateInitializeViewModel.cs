namespace Template.MobileApp.Modules.Navigation.Navigate;

public sealed class NavigateInitializeViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    public NavigateInitializeViewModel(
        IDialog dialog)
    {
        this.dialog = dialog;
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            await Navigator.PostActionAsync(InitializeAsync);
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    private Task InitializeAsync()
    {
        return BusyState.UsingAsync(async () =>
        {
            using (dialog.Loading("Initializing..."))
            {
                await Task.Delay(3000);
            }
        });
    }
}
