namespace Template.MobileApp;

using MauiPermissions = Microsoft.Maui.ApplicationModel.Permissions;

public sealed class ActivityRecognition : MauiPermissions.BasePlatformPermission
{
#if ANDROID
    public override (string, bool)[] RequiredPermissions =>
        [(global::Android.Manifest.Permission.ActivityRecognition, true)];
#endif
}

// Android 13 以降の Wi-Fi スキャンに必要 (それ以前は位置情報のみ)
public sealed class NearbyWifiDevices : MauiPermissions.BasePlatformPermission
{
#if ANDROID
    public override (string, bool)[] RequiredPermissions =>
        OperatingSystem.IsAndroidVersionAtLeast(33) ? [(global::Android.Manifest.Permission.NearbyWifiDevices, true)] : [];
#endif
}

#pragma warning disable CA1724
public static class Permissions
{
    public static ValueTask<bool> RequestCameraAsync() =>
        CheckAndRequestAsync<MauiPermissions.Camera>();

    public static ValueTask<bool> RequestMicrophoneAsync() =>
        CheckAndRequestAsync<MauiPermissions.Microphone>();

    // バックグラウンド測位機能は存在しないためLocationWhenInUseで足りる
    public static ValueTask<bool> RequestLocationAsync() =>
        CheckAndRequestAsync<MauiPermissions.LocationWhenInUse>();

    public static ValueTask<bool> RequestActivityRecognitionAsync() =>
        CheckAndRequestAsync<ActivityRecognition>();

    public static ValueTask<bool> RequestNearbyWifiDevicesAsync() =>
        CheckAndRequestAsync<NearbyWifiDevices>();

    // Android 13 以降の通知の表示許可 (それ以前は常に許可)
    public static ValueTask<bool> RequestNotificationsAsync() =>
        CheckAndRequestAsync<MauiPermissions.PostNotifications>();

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
