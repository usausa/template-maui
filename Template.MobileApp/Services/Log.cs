namespace Template.MobileApp.Services;

using Rester;

// 通信系 (HTTP / SignalR / gRPC) のログ
internal static partial class Log
{
    // Network

    [LoggerMessage(Level = LogLevel.Warning, Message = "Network operation failed. result=[{restResult}], statusCode=[{statusCode}]")]
    public static partial void WarnNetworkOperationFailed(this ILogger logger, RestResult restResult, int statusCode, Exception? exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Login. id=[{id}], expires=[{expires}]")]
    public static partial void InfoLogin(this ILogger logger, string id, DateTime? expires);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Login failed. id=[{id}], result=[{restResult}], statusCode=[{statusCode}]")]
    public static partial void WarnLoginFailed(this ILogger logger, string id, RestResult restResult, int statusCode);

    // Monitor (SignalR)

    [LoggerMessage(Level = LogLevel.Information, Message = "Monitor connected. id=[{connectionId}]")]
    public static partial void InfoMonitorConnected(this ILogger logger, string? connectionId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Monitor connect failed. retrying.")]
    public static partial void WarnMonitorConnectFailed(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Monitor reconnecting.")]
    public static partial void WarnMonitorReconnecting(this ILogger logger, Exception? exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Monitor stopped.")]
    public static partial void InfoMonitorStopped(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Monitor connection error.")]
    public static partial void WarnMonitorError(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Monitor notified. title=[{title}]")]
    public static partial void InfoMonitorNotified(this ILogger logger, string title);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Monitor report failed.")]
    public static partial void WarnMonitorReportFailed(this ILogger logger, Exception exception);

    // Chat (gRPC)

    [LoggerMessage(Level = LogLevel.Information, Message = "Chat connected. address=[{address}]")]
    public static partial void InfoChatConnected(this ILogger logger, Uri address);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Chat disconnected.")]
    public static partial void WarnChatDisconnected(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Chat stopped.")]
    public static partial void InfoChatDisconnected(this ILogger logger);
}
