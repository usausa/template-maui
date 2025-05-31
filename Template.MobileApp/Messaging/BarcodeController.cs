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
