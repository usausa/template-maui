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
