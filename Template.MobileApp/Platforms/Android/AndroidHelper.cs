#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.App;

public static class AndroidHelper
{
    public static string GetExternalFilesDir() =>
        Application.Context.GetExternalFilesDir(string.Empty)!.Path;

    public static void MoveTaskToBack() =>
        ActivityResolver.CurrentActivity.MoveTaskToBack(true);
}
