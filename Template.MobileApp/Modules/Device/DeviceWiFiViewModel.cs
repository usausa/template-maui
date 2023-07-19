namespace Template.MobileApp.Modules.Device;

public class DeviceWiFiViewModel : AppViewModelBase
{
    public DeviceWiFiViewModel(ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
