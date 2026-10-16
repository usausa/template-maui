namespace Template.MobileApp;

using MauiPermissions = Microsoft.Maui.ApplicationModel.Permissions;

public sealed class ActivityRecognition : MauiPermissions.BasePlatformPermission
{
#if ANDROID
    public override (string, bool)[] RequiredPermissions => [(Android.Manifest.Permission.ActivityRecognition, true)];
#endif
}

public sealed class NearbyWifiDevices : MauiPermissions.BasePlatformPermission
{
#if ANDROID
    public override (string, bool)[] RequiredPermissions => OperatingSystem.IsAndroidVersionAtLeast(33) ? [(Android.Manifest.Permission.NearbyWifiDevices, true)] : [];
#endif
}

#pragma warning disable CA1724
public static class Permissions
{
    public static ValueTask<bool> RequestCameraAsync() => CheckAndRequestAsync<MauiPermissions.Camera>();

    public static ValueTask<bool> RequestMicrophoneAsync() => CheckAndRequestAsync<MauiPermissions.Microphone>();

    public static ValueTask<bool> RequestLocationAsync() => CheckAndRequestAsync<MauiPermissions.LocationWhenInUse>();

    public static ValueTask<bool> RequestActivityRecognitionAsync() => CheckAndRequestAsync<ActivityRecognition>();

    public static ValueTask<bool> RequestNearbyWifiDevicesAsync() => CheckAndRequestAsync<NearbyWifiDevices>();

    public static ValueTask<bool> RequestNotificationsAsync() => CheckAndRequestAsync<MauiPermissions.PostNotifications>();

    private static async ValueTask<bool> CheckAndRequestAsync<TPermission>()
        where TPermission : MauiPermissions.BasePermission, new()
    {
        var status = await MauiPermissions.CheckStatusAsync<TPermission>();
        if (status != PermissionStatus.Granted)
        {
            status = await MauiPermissions.RequestAsync<TPermission>();
        }

        return status == PermissionStatus.Granted;
    }
}
#pragma warning restore CA1724
