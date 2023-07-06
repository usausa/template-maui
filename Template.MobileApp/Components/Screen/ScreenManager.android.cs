namespace Template.MobileApp.Components.Screen;

using Android.Content.PM;

public sealed partial class ScreenManager
{
    // ------------------------------------------------------------
    // Display
    // ------------------------------------------------------------

    public void SetOrientation(DisplayOrientation orientation)
    {
        var current = GetOrientation();
        if (current == orientation)
        {
            return;
        }

        var activity = ActivityResolver.CurrentActivity;
        activity.RequestedOrientation = orientation switch
        {
            DisplayOrientation.Landscape => ScreenOrientation.Landscape,
            DisplayOrientation.Portrait => ScreenOrientation.Portrait,
            _ => activity.RequestedOrientation
        };
    }
}
