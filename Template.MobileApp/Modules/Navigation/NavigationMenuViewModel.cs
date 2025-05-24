namespace Template.MobileApp.Modules.Navigation;

public sealed class NavigationMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }
    public IObserveCommand SharedCommand { get; }
    public IObserveCommand DialogCommand { get; }

    public NavigationMenuViewModel(
        IDialog dialog,
        IPopupNavigator popupNavigator)
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
