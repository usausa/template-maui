namespace Template.MobileApp.Diagnostics;

using Android.Runtime;

public static partial class CrashReport
{
    private static partial void PlatformStart()
    {
        AndroidEnvironment.UnhandledExceptionRaiser += static (_, args) => LogException(args.Exception, !args.Handled);
    }

    private static partial string ResolveCrashPath() =>
        Path.Combine(AndroidHelper.GetExternalFilesDir(), "crash.json");
}
