namespace Template.MobileApp.Modules.Network;

using BarcodeScanning;

using Template.MobileApp.Services;

public sealed partial class NetworkSettingViewModel : AppViewModelBase
{
    public BarcodeController Controller { get; } = new();

    [ObservableProperty]
    public partial string Current { get; set; }

    public IObserveCommand DetectCommand { get; }

    public NetworkSettingViewModel(
        ApiContext apiContext,
        Settings settings,
        IDialog dialog)
    {
        Controller.AimMode = true;
        Controller.VibrationOnDetect = true;
        Controller.CaptureNextFrame = false;

        Current = settings.ApiEndPoint;

        DetectCommand = MakeAsyncCommand<IReadOnlySet<BarcodeResult>>(async x =>
        {
            if (Controller.PauseScanning)
            {
                return;
            }

            if (x.Count > 0)
            {
                Controller.PauseScanning = true;

                var barcode = x.First().DisplayValue;
                try
                {
                    var url = new Uri(barcode);
                    if (await dialog.ConfirmAsync($"Update ?\n{barcode}"))
                    {
                        settings.ApiEndPoint = barcode;
                        apiContext.BaseAddress = url;

                        await Navigator.ForwardAsync(ViewId.NetworkMenu);
                        return;
                    }
                }
                catch (UriFormatException)
                {
                    await dialog.InformationAsync("Invalid url.");
                }

                Controller.PauseScanning = false;
            }
        });
    }

    public override void OnNavigatedTo(INavigationContext context)
    {
        Controller.Enable = true;
    }

    public override void OnNavigatingFrom(INavigationContext context)
    {
        Controller.Enable = false;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
