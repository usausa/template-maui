namespace Template.MobileApp.Diagnostics;

public sealed class TelemetryLoggerProvider : ILoggerProvider
{
    private TelemetryService? service;

    public void Dispose() => Volatile.Write(ref service, null);

    public ILogger CreateLogger(string categoryName) => new DelegatingLogger(this, categoryName);

    internal void Attach(TelemetryService value) => Volatile.Write(ref service, value);

    // ------------------------------------------------------------
    // Logger
    // ------------------------------------------------------------

    private sealed class DelegatingLogger : ILogger
    {
        private readonly TelemetryLoggerProvider provider;

        private readonly string category;

        public DelegatingLogger(TelemetryLoggerProvider provider, string category)
        {
            this.provider = provider;
            this.category = category;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull =>
            Volatile.Read(ref provider.service)?.BeginLogScope(category, state);

        public bool IsEnabled(LogLevel logLevel) =>
            Volatile.Read(ref provider.service)?.IsLogEnabled(category, logLevel) ?? false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Volatile.Read(ref provider.service)?.WriteLog(category, logLevel, eventId, state, exception, formatter);
    }
}
