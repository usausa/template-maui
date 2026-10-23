namespace Template.MobileApp.Extender;

using Android.Content;
using Android.Views.InputMethods;

public sealed partial class NavigationFocusPlugin
{
    private static partial void HideSoftInput()
    {
        var activity = ActivityResolver.CurrentActivity;
        var token = activity.Window?.DecorView.WindowToken;
        if ((token is not null) && (activity.GetSystemService(Context.InputMethodService) is InputMethodManager manager))
        {
            manager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
        }
    }
}
