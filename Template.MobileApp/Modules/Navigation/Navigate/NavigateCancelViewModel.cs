namespace Template.MobileApp.Modules.Navigation.Navigate;

public sealed class NavigateCancelViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    public NavigateCancelViewModel(
        IDialog dialog)
    {
        this.dialog = dialog;
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            await Navigator.PostActionAsync(() => BusyState.Using(async () =>
            {
                if (await dialog.ConfirmAsync("Cancel ?", ok: "Yes", cancel: "No"))
                {
                    await Navigator.ForwardAsync(ViewId.NavigationMenu);
                }
            }));
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
