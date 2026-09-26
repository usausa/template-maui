namespace Template.MobileApp.Diagnostics;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed record CrashInfo
{
    public required string Id { get; init; }

    public required DateTimeOffset Time { get; init; }

    public required string Version { get; init; }

    public required string Device { get; init; }

    public required string ExceptionType { get; init; }

    public required string Message { get; init; }

    public required string Detail { get; init; }

    public bool Shown { get; init; }

    public string ToReport()
    {
        var report = new StringBuilder();
        report.AppendLine($"Time: {Time.LocalDateTime:yyyy/MM/dd HH:mm:ss}");
        report.AppendLine($"Version: {Version}");
        report.AppendLine($"Device: {Device}");
        report.AppendLine("Exception:");
        report.AppendLine(Detail);
        return report.ToString();
    }
}

[JsonSerializable(typeof(CrashInfo))]
internal sealed partial class CrashInfoJsonContext : JsonSerializerContext;

public sealed class CrashEventArgs : EventArgs
{
    public CrashInfo Info { get; }

    public bool IsTerminating { get; }

    public CrashEventArgs(CrashInfo info, bool isTerminating)
    {
        Info = info;
        IsTerminating = isTerminating;
    }
}

public static partial class CrashReport
{
    private static Exception? lastException;

    public static event EventHandler<CrashEventArgs>? Crashed;

    public static void Start()
    {
        AppDomain.CurrentDomain.UnhandledException += static (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                LogException(ex, args.IsTerminating);
            }
        };
        TaskScheduler.UnobservedTaskException += static (_, args) => LogException(args.Exception, false);

        PlatformStart();
    }

    private static partial void PlatformStart();

    private static partial string ResolveCrashPath();

    public static void LogException(Exception e, bool isTerminating)
    {
        // The same exception is reported by more than one hook
        if (ReferenceEquals(Interlocked.Exchange(ref lastException, e), e))
        {
            return;
        }

#pragma warning disable CA1031
        try
        {
            var device = DeviceInfo.Current;
            var info = new CrashInfo
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Version = $"{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})",
                Device = $"{device.Manufacturer} {device.Model} / {device.Platform} {device.VersionString}",
                ExceptionType = e.GetType().FullName ?? e.GetType().Name,
                Message = e.Message,
                Detail = e.ToString()
            };
            Save(info);

            Crashed?.Invoke(null, new CrashEventArgs(info, isTerminating));
        }
        catch
        {
            // Ignore
        }
#pragma warning restore CA1031
    }

    // 未表示のクラッシュを表示して表示済みにする (起動時)
    public static async ValueTask ShowReport()
    {
        if (Load() is not { Shown: false } info)
        {
            return;
        }

        var page = Application.Current?.Windows[0].Page;
        if (page is not null)
        {
            await page.DisplayAlertAsync("Crash report", info.ToReport(), "Close");
        }

        Save(info with { Shown = true });
    }

    public static CrashInfo? GetLastReport() => Load();

    public static void ClearReport() => File.Delete(ResolveCrashPath());

    private static CrashInfo? Load()
    {
        var path = ResolveCrashPath();
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(File.ReadAllText(path), CrashInfoJsonContext.Default.CrashInfo);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void Save(CrashInfo info) =>
        File.WriteAllText(ResolveCrashPath(), JsonSerializer.Serialize(info, CrashInfoJsonContext.Default.CrashInfo));
}
