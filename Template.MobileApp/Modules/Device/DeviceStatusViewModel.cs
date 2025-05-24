namespace Template.MobileApp.Modules.Device;

public sealed class DeviceStatusViewModel : AppViewModelBase
{
    public DeviceState DeviceState { get; }

    public DeviceStatusViewModel(DeviceState deviceState)
    {
        DeviceState = deviceState;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
