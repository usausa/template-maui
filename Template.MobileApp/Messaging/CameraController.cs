namespace Template.MobileApp.Messaging;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

public interface ICameraController : INotifyPropertyChanged
{
    // Property

    public bool IsAvailable { get; set; }

    public bool IsCameraBusy { get; set; }

    public CameraInfo? Selected { get; set; }

    public CameraFlashMode CameraFlashMode { get; set; }

    public Size CaptureResolution { get; set; }

    public float ZoomFactor { get; set; }

    public bool IsTorchOn { get; set; }

    // Attach

    void Attach(CameraView view);

    void Detach();
}

public sealed partial class CameraController : ObservableObject, ICameraController
{
    private CameraView? camera;

    // Property

    [ObservableProperty]
    public partial bool IsAvailable { get; set; }

    [ObservableProperty]
    public partial bool IsCameraBusy { get; set; }

    [ObservableProperty]
    public partial CameraInfo? Selected { get; set; }

    [ObservableProperty]
    public partial CameraFlashMode CameraFlashMode { get; set; } = CameraViewDefaults.CameraFlashMode;

    [ObservableProperty]
    public partial Size CaptureResolution { get; set; } = CameraViewDefaults.ImageCaptureResolution;

    [ObservableProperty]
    public partial float ZoomFactor { get; set; } = CameraViewDefaults.ZoomFactor;

    [ObservableProperty]
    public partial bool IsTorchOn { get; set; }

    // Attach

    void ICameraController.Attach(CameraView view)
    {
        camera = view;
    }

    void ICameraController.Detach()
    {
        camera = null;
    }

    // Message

    public async ValueTask<IReadOnlyList<CameraInfo>> GetAvailableListAsync(CancellationToken token = default)
    {
        if (camera is null)
        {
            return [];
        }

        return await camera.GetAvailableCameras(token);
    }

    public void StartPreview()
    {
        camera?.StartCameraPreview(CancellationToken.None);
    }

    public void StopPreview()
    {
        camera?.StopCameraPreview();
    }

    public async ValueTask<Stream?> CaptureAsync(CancellationToken token = default)
    {
        if (camera is null)
        {
            return null;
        }

        var capture = new CaptureObject(camera);
        return await capture.CaptureAsync(token);
    }

    private sealed class CaptureObject
    {
        private readonly TaskCompletionSource<Stream?> result = new();

        private readonly CameraView view;

        public CaptureObject(CameraView view)
        {
            this.view = view;
        }

        public async ValueTask<Stream?> CaptureAsync(CancellationToken token)
        {
            view.MediaCaptured += OnMediaCaptured;
            view.MediaCaptureFailed += OnMediaCaptureFailed;
            await view.CaptureImage(token);
            return await result.Task;
        }

        private void OnMediaCaptured(object? sender, MediaCapturedEventArgs e) => OnMediaCaptured(e.Media);

        private void OnMediaCaptureFailed(object? sender, MediaCaptureFailedEventArgs e) => OnMediaCaptured(null);

        private void OnMediaCaptured(Stream? stream)
        {
            view.MediaCaptured -= OnMediaCaptured;
            view.MediaCaptureFailed -= OnMediaCaptureFailed;
            result.TrySetResult(stream);
        }
    }
}

public static class CameraControllerExtensions
{
    public static async ValueTask SwitchCameraAsync(this CameraController controller)
    {
        var list = await controller.GetAvailableListAsync();
        if (controller.Selected is null)
        {
            controller.Selected = list.ElementAtOrDefault(0);
        }
        else
        {
            var index = list.FindIndex(x => x.DeviceId == controller.Selected.DeviceId);
            if ((index < 0) || (index == list.Count - 1))
            {
                controller.Selected = list.ElementAtOrDefault(0);
            }
            else
            {
                controller.Selected = list[index + 1];
            }
        }
    }

    public static void ToggleTorch(this CameraController controller)
    {
        controller.IsTorchOn = !controller.IsTorchOn;
    }

    public static void SwitchFlashMode(this CameraController controller)
    {
        if (controller.Selected?.IsFlashSupported ?? false)
        {
            controller.CameraFlashMode = controller.CameraFlashMode switch
            {
                CameraFlashMode.Off => CameraFlashMode.On,
                CameraFlashMode.On => CameraFlashMode.Auto,
                CameraFlashMode.Auto => CameraFlashMode.Off,
                _ => controller.CameraFlashMode
            };
        }
    }

    public static void ZoomIn(this CameraController controller)
    {
        var camera = controller.Selected;
        if (camera is null)
        {
            controller.ZoomFactor = 1;
            return;
        }

        controller.ZoomFactor = Math.Min((float)Math.Floor(camera.MaximumZoomFactor), controller.ZoomFactor + 1);
    }

    public static void ZoomOut(this CameraController controller)
    {
        var camera = controller.Selected;
        if (camera is null)
        {
            controller.ZoomFactor = 1;
            return;
        }

        controller.ZoomFactor = Math.Max(1, controller.ZoomFactor - 1);
    }

    public static void SelectMinimumResolution(this CameraController controller)
    {
        var resolutions = controller.Selected?.SupportedResolutions ?? [];
        var size = Size.Zero;
        foreach (var resolution in resolutions)
        {
            if ((resolution.Width < size.Width) || (resolution.Height < size.Height) || size.IsZero)
            {
                size = resolution;
            }
        }

        controller.CaptureResolution = size;
    }
}
