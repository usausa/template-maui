namespace Template.MobileApp.Modules.Key;

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
