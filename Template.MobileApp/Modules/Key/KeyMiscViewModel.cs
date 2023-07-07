namespace Template.MobileApp.Modules.Key;

using Template.MobileApp;

public class KeyMiscViewModel : AppViewModelBase
{
    public KeyMiscViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.KeyMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
