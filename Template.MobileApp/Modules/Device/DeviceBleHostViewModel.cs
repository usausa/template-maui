namespace Template.MobileApp.Modules.Device;

using Shiny;
using Shiny.BluetoothLE.Hosting;

using Template.MobileApp.Providers;

public sealed partial class DeviceBleHostViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IBleHostingManager hostingManager;

    [ObservableProperty]
    public partial string UserId { get; set; }

    [ObservableProperty]
    public partial bool Running { get; set; }

    public DeviceBleHostViewModel(
        IDialog dialog,
        IBleHostingManager hostingManager,
        Settings settings)
    {
        this.dialog = dialog;
        this.hostingManager = hostingManager;

        UserId = settings.UniqId;
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        await Navigator.PostActionAsync(() => BusyState.Using(async () =>
        {
            var access = await hostingManager.RequestAccess();
            if (access == AccessState.Available)
            {
                await SwitchAdvertising(!Running);
            }
            else
            {
                await dialog.InformationAsync("Bluetooth access denied.");
                await Navigator.ForwardAsync(ViewId.DeviceMenu);
            }
        }));
    }

    public override async Task OnNavigatingFromAsync(INavigationContext context)
    {
        if (Running)
        {
            await SwitchAdvertising(false);
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction4()
    {
        await SwitchAdvertising(!Running);
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

        Running = enable;
    }
}
