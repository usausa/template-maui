namespace Template.MobileApp.Modules.Device;

public sealed class DeviceBiometricViewModel : AppViewModelBase
{
    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
