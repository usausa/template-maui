namespace Template.MobileApp;

using MauiPermissions = Microsoft.Maui.ApplicationModel.Permissions;

public sealed class ActivityRecognition : MauiPermissions.BasePlatformPermission
{
#if ANDROID
    public override (string, bool)[] RequiredPermissions =>
        [(global::Android.Manifest.Permission.ActivityRecognition, true)];
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
