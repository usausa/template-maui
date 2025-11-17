namespace Template.MobileApp.Helpers;

public static partial class CrashReport
{
    public static void Start() => PlatformStart();

    private static partial void PlatformStart();

    private static partial string ResolveCrashLogPath();

    private static partial string ResolveOldCrashLogPath();

    public static void LogException(Exception e)
    {
#pragma warning disable CA1031
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
#pragma warning restore CA1031
    }

    public static async ValueTask ShowReport()
    {
        var path = ResolveCrashLogPath();
        if (!File.Exists(path))
        {
            return;
        }

        var log = await File.ReadAllTextAsync(path);
        var page = Application.Current?.Windows[0].Page;
        if (page is not null)
        {
            await page.DisplayAlertAsync("Crash report", log, "Close");
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
