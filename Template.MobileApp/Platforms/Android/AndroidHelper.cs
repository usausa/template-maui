// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.App;
using Android.Views;

public static class AndroidHelper
{
    public static void FullScreen(Activity activity)
    {
        var window = activity.Window!;
        window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
    }

    public static string GetExternalFilesDir() =>
        Application.Context.GetExternalFilesDir(string.Empty)!.Path;
}
