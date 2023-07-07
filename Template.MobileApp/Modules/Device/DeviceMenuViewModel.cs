namespace Template.MobileApp.Modules.Device;

using Template.MobileApp;

public class DeviceMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }

    public DeviceMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
