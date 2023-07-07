namespace Template.MobileApp.Modules.Key;

using Template.MobileApp;

public class KeyListViewModel : AppViewModelBase
{
    public KeyListViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.KeyMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
