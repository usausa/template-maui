namespace Template.MobileApp.Modules.Device;

using BarcodeScanning;

using Template.MobileApp.Graphics;

public sealed partial class DeviceQrScanViewModel : AppViewModelBase
{
    public BarcodeController Controller { get; } = new();

    public BarcodeGraphics Graphics { get; } = new();

    [ObservableProperty]
    public partial string Barcode { get; set; } = string.Empty;

    public IObserveCommand TorchCommand { get; }
    public IObserveCommand AimCommand { get; }
    public IObserveCommand ZoomOutCommand { get; }
    public IObserveCommand ZoomInCommand { get; }

    public IObserveCommand DetectCommand { get; }

    public DeviceQrScanViewModel()
    {
        Controller.TapToFocus = true;

        TorchCommand = MakeDelegateCommand(Controller.ToggleTorch);
        AimCommand = MakeDelegateCommand(Controller.ToggleAimMode);
        ZoomOutCommand = MakeDelegateCommand(Controller.ZoomOut);
        ZoomInCommand = MakeDelegateCommand(Controller.ZoomIn);

        DetectCommand = MakeDelegateCommand<IReadOnlySet<BarcodeResult>>(x =>
        {
            Graphics.Update(x);

            if (x.Count > 0)
            {
                Controller.PauseScanning = true;

                Barcode = x.First().DisplayValue;

                Controller.PauseScanning = false;
            }
        });
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        Controller.Enable = true;
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        Controller.Enable = false;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Controller.SwitchCameraFace();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        Controller.ToggleForceInvert();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        Controller.ToggleVibrationOnDetect();
        return Task.CompletedTask;
    }
}
