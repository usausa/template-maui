namespace Template.MobileApp.Messaging;

using BarcodeScanning;

public sealed partial class BarcodeController : ObservableObject
{
    [ObservableProperty]
    public partial bool Enable { get; set; }

    [ObservableProperty]
    public partial CameraFacing CameraFace { get; set; }

    [ObservableProperty]
    public partial CaptureQuality CaptureQuality { get; set; } = CaptureQuality.Medium;

    [ObservableProperty]
    public partial BarcodeFormats BarcodeFormat { get; set; } = BarcodeFormats.All;

    [ObservableProperty]
    public partial bool AimMode { get; set; }

    [ObservableProperty]
    public partial bool TapToFocus { get; set; }

    [ObservableProperty]
    public partial bool TorchOn { get; set; }

    [ObservableProperty]
    public partial bool PauseScanning { get; set; }

    [ObservableProperty]
    public partial bool ForceInvert { get; set; }

    [ObservableProperty]
    public partial bool VibrationOnDetect { get; set; }

    [ObservableProperty]
    public partial bool ViewfinderMode { get; set; }

    [ObservableProperty]
    public partial bool CaptureNextFrame { get; set; }

    [ObservableProperty]
    public partial bool ForceFrameCapture { get; set; }

    [ObservableProperty]
    public partial int PoolingInterval { get; set; }

    [ObservableProperty]
    public partial float RequestZoomFactor { get; set; } = -1f;

    // Readonly

    [ObservableProperty]
    public partial float CurrentZoomFactor { get; set; } = -1f;

    [ObservableProperty]
    public partial float MinZoomFactor { get; set; } = -1f;

    [ObservableProperty]
    public partial float MaxZoomFactor { get; set; } = -1f;

    [ObservableProperty]
    public partial float DeviceSwitchZoomFactor { get; set; } = -1f;
}

public static class BarcodeControllerExtensions
{
    public static void SwitchCameraFace(this BarcodeController controller)
    {
        controller.CameraFace = controller.CameraFace switch
        {
            CameraFacing.Back => CameraFacing.Front,
            CameraFacing.Front => CameraFacing.Back,
            _ => controller.CameraFace
        };
    }

    public static void ToggleAimMode(this BarcodeController controller)
    {
        controller.AimMode = !controller.AimMode;
    }

    public static void ToggleTapToFocus(this BarcodeController controller)
    {
        controller.TapToFocus = !controller.TapToFocus;
    }

    public static void ToggleTorch(this BarcodeController controller)
    {
        controller.TorchOn = !controller.TorchOn;
    }

    public static void ToggleForceInvert(this BarcodeController controller)
    {
        controller.ForceInvert = !controller.ForceInvert;
    }

    public static void ToggleVibrationOnDetect(this BarcodeController controller)
    {
        controller.VibrationOnDetect = !controller.VibrationOnDetect;
    }

    public static void ToggleViewfinderMode(this BarcodeController controller)
    {
        controller.ViewfinderMode = !controller.ViewfinderMode;
    }

    public static void ToggleCaptureNextFrame(this BarcodeController controller)
    {
        controller.CaptureNextFrame = !controller.CaptureNextFrame;
    }

    public static void ToggleForceFrameCapture(this BarcodeController controller)
    {
        controller.ForceFrameCapture = !controller.ForceFrameCapture;
    }

    public static void ZoomIn(this BarcodeController controller)
    {
        controller.RequestZoomFactor = Math.Min((float)Math.Floor(controller.MaxZoomFactor), controller.CurrentZoomFactor + 1);
    }

    public static void ZoomOut(this BarcodeController controller)
    {
        controller.RequestZoomFactor = Math.Max(1, controller.CurrentZoomFactor - 1);
    }
}
