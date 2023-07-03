namespace Template.MobileApp.Modules.Device;

public class DeviceInfoViewModel : AppViewModelBase
{
    public ICommand BackCommand { get; }

    public DeviceInfoViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.DeviceMenu));
    }
}
