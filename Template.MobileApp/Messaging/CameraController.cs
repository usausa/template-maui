namespace Template.MobileApp.Messaging;

using CommunityToolkit.Maui.Core;

using Smart.Linq;

using Template.MobileApp.Helpers;

public sealed class CameraPreviewEventArgs : ValueTaskEventArgs
{
    public bool Enable { get; set; }
}

public sealed class CameraCaptureEventArgs : ValueTaskEventArgs<Stream?>
{
    public CancellationToken Token { get; set; } = CancellationToken.None;
}

public sealed class CameraGetAvailableListEventArgs : ValueTaskEventArgs<IReadOnlyList<CameraInfo>>
{
    public CancellationToken Token { get; set; } = CancellationToken.None;

    public IReadOnlyList<CameraInfo> CameraList { get; set; } = [];
}

public sealed partial class CameraController : ObservableObject
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<CameraGetAvailableListEventArgs>? GetAvailableListRequest;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<CameraPreviewEventArgs>? PreviewRequest;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<CameraCaptureEventArgs>? CaptureRequest;

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

    // Message

    public ValueTask<IReadOnlyList<CameraInfo>> GetAvailableListAsync(CancellationToken token = default)
    {
        var args = new CameraGetAvailableListEventArgs
        {
            Token = token,
            CameraList = []
        };
        GetAvailableListRequest?.Invoke(this, args);
        return args.Task;
    }

    public ValueTask StartPreviewAsync()
    {
        var args = new CameraPreviewEventArgs { Enable = true };
        PreviewRequest?.Invoke(this, args);
        return args.Task;
    }

    public ValueTask StopPreviewAsync()
    {
        var args = new CameraPreviewEventArgs();
        PreviewRequest?.Invoke(this, args);
        return args.Task;
    }

    public ValueTask<Stream?> CaptureAsync(CancellationToken token = default)
    {
        var args = new CameraCaptureEventArgs
        {
            Token = token
        };
        CaptureRequest?.Invoke(this, args);
        return args.Task;
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
