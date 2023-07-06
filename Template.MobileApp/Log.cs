namespace Template.MobileApp;

#pragma warning disable SYSLIB1006
public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Application start.")]
    public static partial void InfoApplicationStart(this ILogger logger);
}
#pragma warning restore SYSLIB1006
