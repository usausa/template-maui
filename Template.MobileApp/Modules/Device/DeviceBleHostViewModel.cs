namespace Template.MobileApp.Modules.Device;

using Shiny;
using Shiny.BluetoothLE.Hosting;

using Template.MobileApp.Providers;

public class DeviceBleHostViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IBleHostingManager hostingManager;

    public NotificationValue<string> UserId { get; } = new();

    public NotificationValue<bool> Running { get; } = new();

    public DeviceBleHostViewModel(
        ApplicationState applicationState,
        IDialog dialog,
        IBleHostingManager hostingManager,
        Settings settings)
        : base(applicationState)
    {
        this.dialog = dialog;
        this.hostingManager = hostingManager;

        UserId.Value = settings.UserId;
    }

    // ReSharper disable once AsyncVoidMethod
    public override async void OnNavigatedTo(INavigationContext context)
    {
        await Navigator.PostActionAsync(() => BusyState.Using(async () =>
        {
            var access = await hostingManager.RequestAccess();
            if (access == AccessState.Available)
            {
                await SwitchAdvertising(!Running.Value);
            }
            else
            {
                await dialog.InformationAsync("Bluetooth access denied.");
                await Navigator.ForwardAsync(ViewId.DeviceMenu);
            }
        }));
    }

    // ReSharper disable once AsyncVoidMethod
    public override async void OnNavigatingFrom(INavigationContext context)
    {
        if (Running.Value)
        {
            await SwitchAdvertising(false);
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction4()
    {
        await SwitchAdvertising(!Running.Value);
    }

    private async ValueTask SwitchAdvertising(bool enable)
    {
        if (hostingManager.IsAdvertising == enable)
        {
            return;
        }

        if (enable)
        {
            if (!hostingManager.IsRegisteredServicesAttached)
            {
                await hostingManager.AttachRegisteredServices();
            }

            await hostingManager.StartAdvertising(new AdvertisementOptions(BleConstants.LocalName, BleConstants.UserServiceUuid));
        }
        else
        {
            hostingManager.StopAdvertising();
            hostingManager.DetachRegisteredServices();
        }

        Running.Value = enable;
    }
}
