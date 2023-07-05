namespace Template.MobileApp.Components.Device;

using Android.Content.PM;

public sealed partial class DeviceManager
{
    // ------------------------------------------------------------
    // Display
    // ------------------------------------------------------------

    public void SetOrientation(Orientation orientation)
    {
        var current = GetOrientation();
        if (current == orientation)
        {
            return;
        }

        var activity = ActivityResolver.CurrentActivity;
        activity.RequestedOrientation = orientation switch
        {
            Orientation.Landscape => ScreenOrientation.Landscape,
            Orientation.Portrait => ScreenOrientation.Portrait,
            _ => activity.RequestedOrientation
        };
    }
}
