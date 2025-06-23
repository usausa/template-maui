namespace Template.MobileApp;

public static class Permissions
{
    public static async ValueTask<bool> RequestCameraAsync()
    {
        var status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.Camera>();
        return status is PermissionStatus.Granted;
    }

    public static async ValueTask<bool> RequestMicrophoneAsync()
    {
        var status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.Microphone>();
        return status is PermissionStatus.Granted;
    }

    public static async ValueTask<bool> RequestLocationAsync()
    {
        var status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationAlways>();
        return status is PermissionStatus.Granted;
    }
}
