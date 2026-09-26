namespace Template.MobileApp;

internal static partial class Log
{
    // Startup

    [LoggerMessage(Level = LogLevel.Information, Message = "Application start. version=[{version}], runtime=[{runtime}]")]
    public static partial void InfoApplicationStart(this ILogger logger, Version? version, Version runtime);

    [LoggerMessage(Level = LogLevel.Error, Message = "Database initialize failed.")]
    public static partial void ErrorDatabaseInitializeFailed(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Font warmup completed. target=[{target}], elapsed=[{elapsed}]")]
    public static partial void DebugFontWarmup(this ILogger logger, string target, long elapsed);

    // State

    [LoggerMessage(Level = LogLevel.Debug, Message = "Screen state changed. state=[{on}]")]
    public static partial void DebugScreenStateChanged(this ILogger logger, bool on);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Battery info changed. level=[{chargeLevel}], state=[{state}], source=[{powerSource}]")]
    public static partial void DebugBatteryState(this ILogger logger, double chargeLevel, Microsoft.Maui.Devices.BatteryState state, BatteryPowerSource powerSource);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Connectivity changed. profile=[{profile}], access=[{access}]")]
    public static partial void DebugConnectivityState(this ILogger logger, NetworkProfile profile, NetworkAccess access);

    // Diagnostics

    [LoggerMessage(Level = LogLevel.Information, Message = "Telemetry endpoint changed. endpoint=[{endpoint}]")]
    public static partial void InfoTelemetryEndPointChanged(this ILogger logger, Uri? endpoint);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Diagnostic sampler changed. running=[{running}]")]
    public static partial void DebugSamplerChanged(this ILogger logger, bool running);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Telemetry operation failed.")]
    public static partial void WarnTelemetryOperationFailed(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Telemetry SDK event. source=[{source}], level=[{level}], message=[{message}]")]
    public static partial void WarnTelemetrySdkEvent(this ILogger logger, string source, System.Diagnostics.Tracing.EventLevel level, string message);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Telemetry send failed. reason=[{reason}]")]
    public static partial void WarnTelemetrySendFailed(this ILogger logger, string? reason);

    [LoggerMessage(Level = LogLevel.Information, Message = "Telemetry send recovered.")]
    public static partial void InfoTelemetrySendRecovered(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Critical, EventName = "exception", Message = "Application crashed. time=[{time}]")]
    public static partial void CriticalApplicationCrashed(this ILogger logger, DateTimeOffset time);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Telemetry test warning.")]
    public static partial void WarnTelemetryTest(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Telemetry test error.")]
    public static partial void ErrorTelemetryTest(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Telemetry test span completed. elapsed=[{elapsed}]")]
    public static partial void WarnTelemetryTestSpan(this ILogger logger, long elapsed);

    // Navigation

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unhandled navigation error.")]
    public static partial void WarnUnhandledNavigationError(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Leak suspected. target=[{target}]")]
    public static partial void WarnLeakSuspected(this ILogger logger, string target);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Closed object collected. target=[{target}]")]
    public static partial void DebugClosedObjectCollected(this ILogger logger, string target);

    // Device

    [LoggerMessage(Level = LogLevel.Warning, Message = "BLE scan error.")]
    public static partial void WarnBleScanError(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Nfc tag read error.")]
    public static partial void WarnNfcReadError(this ILogger logger, Exception exception);
}
