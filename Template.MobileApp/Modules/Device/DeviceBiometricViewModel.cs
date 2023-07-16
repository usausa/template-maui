namespace Template.MobileApp.Modules.Device;

using Template.MobileApp;

public class DeviceBiometricViewModel : AppViewModelBase
{
    public DeviceBiometricViewModel(ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
