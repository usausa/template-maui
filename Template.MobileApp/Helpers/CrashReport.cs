namespace Template.MobileApp.Helpers;

public static partial class CrashReport
{
    public static void Start() => PlatformStart();

    private static partial void PlatformStart();

    private static partial string ResolveCrashLogPath();

    private static partial string ResolveOldCrashLogPath();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore")]
    private static void LogException(Exception e)
    {
        try
        {
            var path = ResolveCrashLogPath();

            var log = new StringBuilder();
            log.AppendLine($"Time: {DateTime.Now:yyyy/MM/dd HH:mm:ss}");
            log.AppendLine("Exception:");
            log.AppendLine(e.ToString());

            File.WriteAllText(path, log.ToString());
        }
        catch
        {
            // Ignore
        }
    }

    public static async ValueTask ShowReport()
    {
        var path = ResolveCrashLogPath();
        if (!File.Exists(path))
        {
            return;
        }

        var log = await File.ReadAllTextAsync(path);
        if (Application.Current?.MainPage is not null)
        {
            await Application.Current.MainPage.DisplayAlert("Crash report", log, "Close");
        }

        var oldPath = ResolveOldCrashLogPath();
        File.Move(path, oldPath, true);
    }

    public static string? GetReport()
    {
        var path = ResolveCrashLogPath();
        return !File.Exists(path) ? null : File.ReadAllText(path);
    }
}
