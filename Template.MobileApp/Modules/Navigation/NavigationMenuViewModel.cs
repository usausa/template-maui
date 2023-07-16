namespace Template.MobileApp.Modules.Navigation;

using Template.MobileApp;

public class NavigationMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }
    public ICommand SharedCommand { get; }
    public ICommand DialogCommand { get; }

    public NavigationMenuViewModel(
        ApplicationState applicationState,
        IDialog dialog,
        IPopupNavigator popupNavigator)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
        SharedCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(ViewId.NavigationSharedInput, Parameters.MakeNextViewId(x)));
        DialogCommand = MakeAsyncCommand(async () =>
        {
            var result = await popupNavigator.InputNumberAsync("Input", "0", 5);
            if (!String.IsNullOrWhiteSpace(result))
            {
                await dialog.InformationAsync($"result={result}");
            }
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
