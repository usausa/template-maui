// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MauiComponents;

using Template.MobileApp.Platforms.Android;

[Activity(
    Name = "template.mobileapp.MainActivity",
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : MauiAppCompatActivity
{
    private KeyInputDriver keyInputDriver = default!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        ActivityResolver.Init(this);
        keyInputDriver = new KeyInputDriver(this);
    }

    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        if (keyInputDriver.Process(e!))
        {
            return true;
        }

        return base.DispatchKeyEvent(e);
    }
}
