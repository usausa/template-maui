namespace Template.MobileApp.Modules.Device;

public class DeviceMenuViewModel : AppViewModelBase
{
    public ICommand BackCommand { get; }

    public DeviceMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.Menu));
    }
}
