namespace Template.MobileApp.Modules.Device;

public class DeviceStatusViewModel : AppViewModelBase
{
    public DeviceState DeviceState { get; }

    public DeviceStatusViewModel(
        ApplicationState applicationState,
        DeviceState deviceState)
        : base(applicationState)
    {
        DeviceState = deviceState;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
