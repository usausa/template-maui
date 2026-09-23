namespace Template.MobileApp.Behaviors;

using Android.Runtime;

using AndroidX.Camera.Camera2.InterOp;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;

using CameraInfo = CommunityToolkit.Maui.Core.CameraInfo;

public static partial class CameraBind
{
    private static partial CameraInfo CorrectCameraInfo(CameraInfo camera)
    {
        if (camera.MaximumZoomFactor > 1f)
        {
            return camera;
        }

        try
        {
            var provider = ProcessCameraProvider.GetInstance(Android.App.Application.Context).Get()?.JavaCast<ProcessCameraProvider>();
            if (provider is null)
            {
                return camera;
            }

            foreach (var info in provider.AvailableCameraInfos)
            {
                if (Camera2CameraInfo.From(info).CameraId != camera.DeviceId)
                {
                    continue;
                }

                var zoom = info.ZoomState?.Value?.JavaCast<IZoomState>();
                return (zoom is null) || (info.CameraSelector is null)
                    ? camera
                    : new CameraInfo(camera.Name, camera.DeviceId, camera.Position, camera.IsFlashSupported, zoom.MinZoomRatio, zoom.MaxZoomRatio, camera.SupportedResolutions, info.CameraSelector);
            }
        }
        catch (Exception ex) when (ex is Java.Lang.Exception or InvalidCastException)
        {
            // Ignore
        }

        return camera;
    }
}
