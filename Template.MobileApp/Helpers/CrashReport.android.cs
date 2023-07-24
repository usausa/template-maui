namespace Template.MobileApp.Helpers;

using Android.Runtime;

public static partial class CrashReport
{
    private static partial void PlatformStart()
    {
        TaskScheduler.UnobservedTaskException += static (_, args) => LogException(args.Exception);
        AndroidEnvironment.UnhandledExceptionRaiser += static (_, args) => LogException(args.Exception);
    }

    private static partial string ResolveCrashLogPath() =>
        Path.Combine(AndroidHelper.GetExternalFilesDir(), "crash.log");

    private static partial string ResolveOldCrashLogPath() =>
        Path.Combine(AndroidHelper.GetExternalFilesDir(), "crash.old.log");
}
