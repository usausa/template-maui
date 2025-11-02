namespace Template.MobileApp.Modules.Network;

public sealed class NetworkGrpcViewModel : AppViewModelBase
{
    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
