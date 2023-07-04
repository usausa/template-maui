namespace Template.MobileApp.Modules.Device;

public class DeviceMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }
    public ICommand BackCommand { get; }

    public DeviceMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.Menu));
    }
}
