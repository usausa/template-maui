namespace Template.MobileApp.Modules.Network;

public class NetworkViewModel : AppViewModelBase
{
    public NetworkViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
