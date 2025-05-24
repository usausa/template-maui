namespace Template.MobileApp.Modules.Navigation.Navigate;

public abstract class NavigateInitializeViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    protected NavigateInitializeViewModel(
        IDialog dialog)
    {
        this.dialog = dialog;
    }

    // ReSharper disable once AsyncVoidMethod
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
