namespace Template.MobileApp.Modules.Device;

public class DeviceBleHostViewModel : AppViewModelBase
{
    public DeviceBleHostViewModel(ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
