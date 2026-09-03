namespace Template.MobileApp;

using Rester;

internal static partial class Log
{
    // Startup

    [LoggerMessage(Level = LogLevel.Information, Message = "Application start. version=[{version}], runtime=[{runtime}]")]
    public static partial void InfoApplicationStart(this ILogger logger, Version? version, Version runtime);

    // State

    [LoggerMessage(Level = LogLevel.Debug, Message = "Screen state changed. state=[{on}]")]
    public static partial void DebugScreenStateChanged(this ILogger logger, bool on);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Battery info changed. level=[{chargeLevel}], state=[{state}], source=[{powerSource}]")]
    public static partial void DebugBatteryState(this ILogger logger, double chargeLevel, Microsoft.Maui.Devices.BatteryState state, BatteryPowerSource powerSource);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Connectivity changed. profile=[{profile}], access=[{access}]")]
    public static partial void DebugConnectivityState(this ILogger logger, NetworkProfile profile, NetworkAccess access);

    // Network

    [LoggerMessage(Level = LogLevel.Warning, Message = "Network operation failed. result=[{restResult}], statusCode=[{statusCode}]")]
    public static partial void WarnNetworkOperationFailed(this ILogger logger, RestResult restResult, int statusCode, Exception? exception);

    // Navigation

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unhandled navigation error.")]
    public static partial void WarnUnhandledNavigationError(this ILogger logger, Exception exception);

    // Device

    [LoggerMessage(Level = LogLevel.Warning, Message = "BLE scan error.")]
    public static partial void WarnBleScanError(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Nfc tag read error.")]
    public static partial void WarnNfcReadError(this ILogger logger, Exception exception);

    // Startup

    [LoggerMessage(Level = LogLevel.Error, Message = "Database initialize failed.")]
    public static partial void ErrorDatabaseInitializeFailed(this ILogger logger, Exception exception);
}
