namespace Template.MobileApp.Diagnostics;

using System.Diagnostics;
using System.Diagnostics.Metrics;

using Template.MobileApp.Components;

public sealed class DiagnosticsInstrumentation : IDisposable
{
    public static readonly string Name = typeof(DiagnosticsInstrumentation).Assembly.GetName().Name!;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMilliseconds(500);

    private static readonly KeyValuePair<string, object?>[] BatteryTags = [new("hw.id", "battery")];

    private readonly int processorCount = Environment.ProcessorCount;

    private readonly Lock sync = new();

    private readonly DeviceInformation deviceInformation;

    private readonly Meter meter;

    private ProcessStatistics last;

    private long heapSize;

    private ProcessStatistics previous;

    public ActivitySource Source { get; } = new(Name);

    public DiagnosticsInstrumentation(
        IMeterFactory meterFactory,
        DeviceInformation deviceInformation)
    {
        this.deviceInformation = deviceInformation;
        meter = meterFactory.Create(Name);
        previous = Read().Statistics;

        // Send lightweight metrics only
        meter.CreateObservableGauge("process.cpu.utilization", ObserveCpuUtilization, "1", "CPU utilization of the process");
        meter.CreateObservableUpDownCounter("process.memory.usage", ObserveWorkingSet, "By", "Working set of the process");
        meter.CreateObservableUpDownCounter("process.thread.count", ObserveThreadCount, "{thread}", "Threads of the process");
        meter.CreateObservableUpDownCounter("application.gc.last_collection.heap.size", ObserveHeapSize, "By", "Managed heap size");
        meter.CreateObservableGauge("hw.battery.charge", ObserveBatteryCharge, "1", "Remaining fraction of battery charge");
        meter.CreateObservableGauge("application.wifi.signal_strength", ObserveWiFiSignalStrength, "dBm", "Signal strength of the connected wireless LAN");
    }

    public void Dispose()
    {
        Source.Dispose();
        meter.Dispose();
    }

    private (ProcessStatistics Statistics, long HeapSize) Read()
    {
        lock (sync)
        {
            var timestamp = Stopwatch.GetTimestamp();
            if ((last.Timestamp != 0) && (Stopwatch.GetElapsedTime(last.Timestamp, timestamp) < CacheDuration))
            {
                return (last, heapSize);
            }

            var statistics = deviceInformation.ReadProcessStatistics();

            // The heap changes only when GC occurs
            if ((last.Timestamp == 0) || (statistics.Gc0Count != last.Gc0Count) || (statistics.Gc1Count != last.Gc1Count) || (statistics.Gc2Count != last.Gc2Count))
            {
                heapSize = DeviceInformation.ReadHeapSize();
            }

            last = statistics;
            return (last, heapSize);
        }
    }

    private IEnumerable<Measurement<double>> ObserveCpuUtilization()
    {
        var statistics = Read().Statistics;
        var elapsed = Stopwatch.GetElapsedTime(previous.Timestamp, statistics.Timestamp).TotalSeconds;
        if (elapsed <= 0)
        {
            return [];
        }

        var utilization = (statistics.CpuTime - previous.CpuTime).TotalSeconds / elapsed / processorCount;
        previous = statistics;
        return [new Measurement<double>(utilization)];
    }

    private long ObserveWorkingSet() => Read().Statistics.WorkingSet;

    private int ObserveThreadCount() => Read().Statistics.ThreadCount;

    private long ObserveHeapSize() => Read().HeapSize;

    private IEnumerable<Measurement<double>> ObserveBatteryCharge()
    {
        var level = deviceInformation.Battery?.Level ?? -1;
        return level is >= 0 and <= 1 ? [new Measurement<double>(level, BatteryTags)] : [];
    }

    private IEnumerable<Measurement<int>> ObserveWiFiSignalStrength() =>
        deviceInformation.WiFi is { } wifi ? [new Measurement<int>(wifi.SignalStrength)] : [];
}
