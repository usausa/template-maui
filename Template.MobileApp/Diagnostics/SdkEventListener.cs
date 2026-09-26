namespace Template.MobileApp.Diagnostics;

using System.Diagnostics.Tracing;

public sealed class SdkEventArgs : EventArgs
{
    public string Source { get; }

    public string Name { get; }

    public EventLevel Level { get; }

    public string Message { get; }

    public SdkEventArgs(string source, string name, EventLevel level, string message)
    {
        Source = source;
        Name = name;
        Level = level;
        Message = message;
    }
}

// Self diagnostics of OpenTelemetry SDK (Warning or higher)
public sealed class SdkEventListener : EventListener
{
    private const string Prefix = "OpenTelemetry";

    // Notification of instruments intentionally dropped in the view
    private const string InstrumentIgnoredEvent = "MetricInstrumentIgnored";

    private const int MaxLength = 300;

    public event EventHandler<SdkEventArgs>? Written;

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name.StartsWith(Prefix, StringComparison.Ordinal))
        {
            EnableEvents(eventSource, EventLevel.Warning);
        }

        base.OnEventSourceCreated(eventSource);
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventName == InstrumentIgnoredEvent)
        {
            return;
        }

        var message = eventData.Message ?? eventData.EventName ?? string.Empty;
        if ((eventData.Payload is { Count: > 0 }) && (eventData.Message is not null))
        {
            try
            {
                message = String.Format(CultureInfo.InvariantCulture, eventData.Message, [.. eventData.Payload]);
            }
            catch (FormatException)
            {
                // Keep the raw message
            }
        }

        var end = message.IndexOfAny(['\r', '\n']);
        if (end >= 0)
        {
            message = message[..end];
        }

        if (message.Length > MaxLength)
        {
            message = message[..MaxLength];
        }

        Written?.Invoke(this, new SdkEventArgs(eventData.EventSource.Name, eventData.EventName ?? string.Empty, eventData.Level, message));
    }
}
