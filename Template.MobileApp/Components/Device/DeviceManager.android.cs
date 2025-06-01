namespace Template.MobileApp.Components.Device;

#pragma warning disable CA1822
public sealed partial class DeviceManager
{
    public partial void SetScreenBrightness(float brightness)
    {
        var activity = ActivityResolver.CurrentActivity;

        if (activity.Window?.Attributes is null)
        {
            return;
        }

        var layoutParams = activity.Window.Attributes;
        layoutParams.ScreenBrightness = brightness;
        activity.Window.Attributes = layoutParams;
    }
}
#pragma warning restore CA1822
