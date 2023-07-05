namespace Template.MobileApp;

public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Application start.")]
    public static partial void InfoApplicationStart(this ILogger logger);
}
