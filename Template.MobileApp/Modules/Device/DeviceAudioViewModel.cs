namespace Template.MobileApp.Modules.Device;

using Template.MobileApp;

public class DeviceAudioViewModel : AppViewModelBase
{
    public DeviceAudioViewModel(ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}