namespace Template.MobileApp.Components.Device;

using Android.Content;
using Android.Content.PM;
using Android.Hardware.Display;
using Android.OS;

public sealed partial class DeviceManager
{
    public void SetOrientation(Orientation orientation)
    {
        var activity = ActivityResolver.CurrentActivity;
        var displayManager = (DisplayManager)activity.GetSystemService(Context.DisplayService)!;
        var display = displayManager.GetDisplay(0)!;
        var displayContext = activity.CreateDisplayContext(display)!;
        var displayMetrics = displayContext.Resources!.DisplayMetrics!;
        var width = displayMetrics.WidthPixels;
        var height = displayMetrics.HeightPixels;

        switch (orientation)
        {
            case Orientation.Landscape:
                if (width < height)
                {
                    activity.RequestedOrientation = ScreenOrientation.Landscape;
                }
                break;
            case Orientation.Portrait:
                if (width > height)
                {
                    activity.RequestedOrientation = ScreenOrientation.Portrait;
                }
                break;
        }
    }

    public string? GetVersion()
    {
        var activity = ActivityResolver.CurrentActivity;
        var pm = activity.PackageManager!;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            var info = pm.GetPackageInfo(activity.PackageName!, (PackageManager.PackageInfoFlags)0);
            return info.VersionName;
        }
        else
        {
#pragma warning disable CS0618
            var info = pm.GetPackageInfo(activity.PackageName!, 0)!;
#pragma warning restore CS0618
            return info.VersionName;
        }
    }
}
