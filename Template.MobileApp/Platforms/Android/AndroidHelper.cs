#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.App;

public static class AndroidHelper
{
    // 外部ストレージ非搭載時は内部ストレージへフォールバック
    public static string GetExternalFilesDir() =>
        Application.Context.GetExternalFilesDir(string.Empty)?.Path ?? Application.Context.FilesDir!.Path;

    public static void MoveTaskToBack() =>
        ActivityResolver.CurrentActivity.MoveTaskToBack(true);
}
