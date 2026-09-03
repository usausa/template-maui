namespace Template.MobileApp.Modules.Device;

using Shiny;
using Shiny.BluetoothLE.Hosting;

using Template.MobileApp.Providers;

public sealed partial class DeviceBleHostViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IBleHostingManager hostingManager;

    private readonly UserCharacteristic userCharacteristic;

    [ObservableProperty]
    public partial string UserId { get; set; }

    [ObservableProperty]
    public partial bool Running { get; set; }

    public DeviceBleHostViewModel(
        IDialog dialog,
        IBleHostingManager hostingManager,
        UserCharacteristic userCharacteristic,
        Settings settings)
    {
        this.dialog = dialog;
        this.hostingManager = hostingManager;
        this.userCharacteristic = userCharacteristic;

        UserId = settings.UniqueId;
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
            if (hostingManager.Services.Count == 0)
            {
                await userCharacteristic.Register(hostingManager);
            }

            await hostingManager.StartAdvertising(new AdvertisementOptions(BleConstants.LocalName, BleConstants.UserServiceUuid));
        }
        else
        {
            hostingManager.StopAdvertising();
            hostingManager.RemoveService(BleConstants.UserServiceUuid);
        }

        Running = enable;
    }
}
