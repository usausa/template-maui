namespace Template.MobileApp.Modules.Device;

public class DeviceStatusViewModel : AppViewModelBase
{
    public ICommand BackCommand { get; }

    public DeviceStatusViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.DeviceMenu));
    }
}
