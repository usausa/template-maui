namespace Template.MobileApp.Modules.Device;

public class DeviceQrDisplayViewModel : AppViewModelBase
{
    public NotificationValue<string> Text { get; } = new();

    public DeviceQrDisplayViewModel(ApplicationState applicationState)
        : base(applicationState)
    {
        Text.Value = "1234567890";
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
