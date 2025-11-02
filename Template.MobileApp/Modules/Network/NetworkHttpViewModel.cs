namespace Template.MobileApp.Modules.Network;

public sealed class NetworkHttpViewModel : AppViewModelBase
{
    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
