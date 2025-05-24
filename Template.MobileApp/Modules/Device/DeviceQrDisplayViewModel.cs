namespace Template.MobileApp.Modules.Device;

public sealed partial class DeviceQrDisplayViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string Text { get; set; }

    public DeviceQrDisplayViewModel()
    {
        Text = "1234567890";
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
