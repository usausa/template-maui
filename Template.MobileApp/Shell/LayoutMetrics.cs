namespace Template.MobileApp.Shell;

using System.Diagnostics.Metrics;

public sealed class LayoutMetrics : IDisposable
{
    private const string MeterName = "Microsoft.Maui";

    private const string ClassIdTag = "element.class_id";

    private readonly MeterListener listener = new();

    private readonly string excludeClassId;

    private long suppressUntil;

    private long measureCount;

    private long arrangeCount;

    private LayoutMetrics(string excludeClassId)
    {
        this.excludeClassId = excludeClassId;

        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == MeterName)
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<int>(OnMeasurement);
        listener.Start();
    }

    public static LayoutMetrics Start(string excludeClassId) => new(excludeClassId);

    public void Dispose()
    {
        listener.Dispose();
    }

    public void Suppress(int milliseconds)
    {
        suppressUntil = Environment.TickCount64 + milliseconds;
    }

    public (long Measure, long Arrange) Take()
    {
        var result = (measureCount, arrangeCount);
        measureCount = 0;
        arrangeCount = 0;
        return result;
    }

    private void OnMeasurement(Instrument instrument, int measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
    {
        if (Environment.TickCount64 < suppressUntil)
        {
            return;
        }

        foreach (var tag in tags)
        {
            if ((tag.Key == ClassIdTag) && (tag.Value is string classId) && (classId == excludeClassId))
            {
                return;
            }
        }

        switch (instrument.Name)
        {
            case "maui.layout.measure_count":
                measureCount += measurement;
                break;
            case "maui.layout.arrange_count":
                arrangeCount += measurement;
                break;
        }
    }
}
