namespace Template.MobileApp.Modules.Device;

public class DeviceCameraViewModel : AppViewModelBase
{
    public CameraController Camera { get; } = new();

    public DeviceCameraViewModel(ApplicationState applicationState)
        : base(applicationState)
    {
        Camera.Preview = true;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction4() => await Camera.SwitchPositionAsync();
}
