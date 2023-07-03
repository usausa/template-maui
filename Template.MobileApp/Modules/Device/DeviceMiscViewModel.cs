namespace Template.MobileApp.Modules.Device;

public class DeviceMiscViewModel : AppViewModelBase
{
    public ICommand BackCommand { get; }

    public DeviceMiscViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.DeviceMenu));
    }
}
