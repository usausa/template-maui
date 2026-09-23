namespace Template.MobileApp.Components;

public sealed record DiagnosticLogEntry(
    DateTime Time,
    LogLevel Level,
    string Category,
    string Message);

public sealed class DiagnosticLogProvider : ILoggerProvider
{
    private const int Capacity = 50;

    private readonly Lock sync = new();

    private readonly DiagnosticLogEntry[] buffer = new DiagnosticLogEntry[Capacity];

    private int next;

    private int count;

    public DiagnosticLogEntry[] GetEntries()
    {
        lock (sync)
        {
            var entries = new DiagnosticLogEntry[count];
            for (var i = 0; i < count; i++)
            {
                entries[i] = buffer[(next - 1 - i + Capacity) % Capacity];
            }
            return entries;
        }
    }

    public void Clear()
    {
        lock (sync)
        {
            Array.Clear(buffer);
            next = 0;
            count = 0;
        }
    }

    public ILogger CreateLogger(string categoryName) => new DiagnosticLogger(this, categoryName);

    public void Dispose()
    {
    }

    private void Add(DiagnosticLogEntry entry)
    {
        lock (sync)
        {
            buffer[next] = entry;
            next = (next + 1) % Capacity;
            if (count < Capacity)
            {
                count++;
            }
        }
    }

    private sealed class DiagnosticLogger : ILogger
    {
        private readonly DiagnosticLogProvider provider;

        private readonly string category;

        public DiagnosticLogger(DiagnosticLogProvider provider, string categoryName)
        {
            this.provider = provider;

            var index = categoryName.LastIndexOf('.');
            category = index >= 0 ? categoryName[(index + 1)..] : categoryName;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => NullScope.Instance;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (exception is not null)
            {
                message = $"{message} ({exception.GetType().Name}: {exception.Message})";
            }

            provider.Add(new DiagnosticLogEntry(DateTime.Now, logLevel, category, message));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
