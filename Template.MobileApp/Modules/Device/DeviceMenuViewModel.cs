namespace Template.MobileApp.Modules.Device;

public sealed class DeviceMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public DeviceMenuViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
