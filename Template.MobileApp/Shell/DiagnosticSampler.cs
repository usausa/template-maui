namespace Template.MobileApp.Shell;

using System.Diagnostics;
using System.Diagnostics.Metrics;

using Template.MobileApp.Components;
using Template.MobileApp.Helpers;

public enum DiagnosticLevel
{
    Safe,
    Warning,
    Critical
}

public sealed class DiagnosticSnapshot
{
    public DateTime Time { get; internal set; }

    // Frame

    public double Fps { get; internal set; }
    public DiagnosticLevel FpsLevel { get; internal set; }

    // CPU

    public double CpuUsage { get; internal set; }
    public DiagnosticLevel CpuLevel { get; internal set; }

    // Thread

    public int ThreadCount { get; internal set; }
    public DiagnosticLevel ThreadLevel { get; internal set; }

    // Memory

    public long WorkingSet { get; internal set; }
    public DiagnosticLevel MemoryLevel { get; internal set; }
    public long ManagedMemory { get; internal set; }
    public RingBuffer<double> MemoryHistory { get; }

    // GC

    public int Gc0Count { get; internal set; }
    public int Gc1Count { get; internal set; }
    public int Gc2Count { get; internal set; }
    public int Gc0Delta { get; internal set; }
    public int Gc1Delta { get; internal set; }
    public int Gc2Delta { get; internal set; }
    public DiagnosticLevel GcLevel { get; internal set; }

    // Allocation

    public double AllocationRate { get; internal set; }
    public DiagnosticLevel AllocationLevel { get; internal set; }

    // Layout

    public long MeasureCount { get; internal set; }
    public DiagnosticLevel MeasureLevel { get; internal set; }
    public long ArrangeCount { get; internal set; }
    public DiagnosticLevel ArrangeLevel { get; internal set; }

    // Battery

    public double? BatteryCharge { get; internal set; }
    public DiagnosticLevel BatteryLevel { get; internal set; }

    // WiFi

    public int? WiFiSignalStrength { get; internal set; }
    public DiagnosticLevel WiFiLevel { get; internal set; }

    internal DiagnosticSnapshot(RingBuffer<double> memoryHistory)
    {
        MemoryHistory = memoryHistory;
    }
}

public sealed class DiagnosticSampler : IDisposable
{
    public const int MemoryHistoryLength = 60;

    private const string LayoutMeterName = "Microsoft.Maui";

    private const string MeasureInstrumentName = "maui.layout.measure_count";
    private const string ArrangeInstrumentName = "maui.layout.arrange_count";

    private const double MegaByte = 1024 * 1024;

    private const double MinFrameTime = 0.1;

    private const double EmaAlpha = 0.9;

    private const string ElementIdTag = "element.id";

    private const int LayoutSuppressMilliseconds = 100;

    // ------------------------------------------------------------
    // Event
    // ------------------------------------------------------------

    public event EventHandler? Sampled;

    // ------------------------------------------------------------
    // Field
    // ------------------------------------------------------------

    private readonly int processorCount = Environment.ProcessorCount;

    private readonly RingBuffer<double> memoryHistory = new(MemoryHistoryLength);

    private readonly HashSet<Guid> excludeElements = [];

    private readonly IDispatcherTimer timer;

    private readonly IDisplay display;

    private MeterListener? layoutListener;

    private readonly DeviceInformation deviceInformation;

    private long layoutSuppressUntil;

    private long measureCount;

    private long arrangeCount;

    private double emaFps;

    private ProcessStatistics previous;

    // ------------------------------------------------------------
    // Property
    // ------------------------------------------------------------

    public bool IsRunning { get; private set; }

    // 同じインスタンスを書き換える
    public DiagnosticSnapshot Snapshot { get; }

    // ------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------

    public DiagnosticSampler(IDisplay display, IDispatcher dispatcher, DeviceInformation deviceInformation)
    {
        this.display = display;
        this.deviceInformation = deviceInformation;
        Snapshot = new DiagnosticSnapshot(memoryHistory);

        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += OnTick;

        display.FrameUpdated += OnFrameUpdated;
    }

    public void Dispose()
    {
        Stop();

        timer.Tick -= OnTick;
        display.FrameUpdated -= OnFrameUpdated;
    }

    // ------------------------------------------------------------
    // Control
    // ------------------------------------------------------------

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        // Read initial statistics to set the baseline
        previous = deviceInformation.ReadProcessStatistics();
        emaFps = 0;
        memoryHistory.Clear();
        measureCount = 0;
        arrangeCount = 0;
        layoutSuppressUntil = 0;
        layoutListener = StartLayoutListener();

        display.StartMonitor();
        timer.Start();

        IsRunning = true;
    }

    private MeterListener StartLayoutListener()
    {
        var listener = new MeterListener
        {
            InstrumentPublished = static (instrument, l) =>
            {
                if (instrument.Meter.Name == LayoutMeterName)
                {
                    l.EnableMeasurementEvents(instrument);
                }
            }
        };
        listener.SetMeasurementEventCallback<int>(OnLayoutMeasurement);
        listener.Start();
        return listener;
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        timer.Stop();
        display.StopMonitor();
        layoutListener?.Dispose();
        layoutListener = null;

        IsRunning = false;
    }

    public void ExcludeLayout(Element element)
    {
        excludeElements.Add(element.Id);
        foreach (var descendant in element.GetVisualTreeDescendants().OfType<Element>())
        {
            excludeElements.Add(descendant.Id);
        }
    }

    // ------------------------------------------------------------
    // Event
    // ------------------------------------------------------------

    private void OnFrameUpdated(double frameTimeMs)
    {
        var fps = 1000.0 / Math.Max(frameTimeMs, MinFrameTime);
        emaFps = emaFps == 0 ? fps : (EmaAlpha * emaFps) + ((1 - EmaAlpha) * fps);
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var statistics = deviceInformation.ReadProcessStatistics();

        var elapsed = Stopwatch.GetElapsedTime(previous.Timestamp, statistics.Timestamp).TotalSeconds;
        if (elapsed <= 0)
        {
            return;
        }

        // CPU
        var cpuUsage = (statistics.CpuTime - previous.CpuTime).TotalSeconds / elapsed * 100 / processorCount;

        // Memory
        memoryHistory.Add(statistics.WorkingSet / MegaByte);

        // Allocation
        var allocationRate = (statistics.AllocatedBytes - previous.AllocatedBytes) / elapsed;

        // GC
        var gc0Delta = statistics.Gc0Count - previous.Gc0Count;
        var gc1Delta = statistics.Gc1Count - previous.Gc1Count;
        var gc2Delta = statistics.Gc2Count - previous.Gc2Count;

        // Layout
        var measure = measureCount;
        var arrange = arrangeCount;
        measureCount = 0;
        arrangeCount = 0;

        // Device
        var batteryCharge = deviceInformation.Battery?.Level;
        var signalStrength = deviceInformation.WiFi?.SignalStrength;

        previous = statistics;

        UpdateTotals(statistics);
        var snapshot = Snapshot;
        snapshot.Fps = emaFps;
        snapshot.FpsLevel = FpsLevelOf(emaFps);
        snapshot.CpuUsage = cpuUsage;
        snapshot.CpuLevel = CpuLevelOf(cpuUsage);
        snapshot.Gc0Delta = gc0Delta;
        snapshot.Gc1Delta = gc1Delta;
        snapshot.Gc2Delta = gc2Delta;
        snapshot.GcLevel = (gc0Delta + gc1Delta + gc2Delta) == 0 ? DiagnosticLevel.Safe : DiagnosticLevel.Critical;
        snapshot.AllocationRate = allocationRate;
        snapshot.AllocationLevel = AllocationLevelOf(allocationRate);
        snapshot.MeasureCount = measure;
        snapshot.MeasureLevel = LayoutLevelOf(measure);
        snapshot.ArrangeCount = arrange;
        snapshot.ArrangeLevel = LayoutLevelOf(arrange);
        snapshot.BatteryCharge = batteryCharge;
        snapshot.BatteryLevel = BatteryLevelOf(batteryCharge);
        snapshot.WiFiSignalStrength = signalStrength;
        snapshot.WiFiLevel = WiFiLevelOf(signalStrength);

        Sampled?.Invoke(this, EventArgs.Empty);

        layoutSuppressUntil = Environment.TickCount64 + LayoutSuppressMilliseconds;
    }

    private void OnLayoutMeasurement(Instrument instrument, int measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
    {
        if (Environment.TickCount64 < layoutSuppressUntil)
        {
            return;
        }

        foreach (var tag in tags)
        {
            if ((tag.Key == ElementIdTag) && (tag.Value is Guid id) && excludeElements.Contains(id))
            {
                return;
            }
        }

        switch (instrument.Name)
        {
            case MeasureInstrumentName:
                measureCount += measurement;
                break;
            case ArrangeInstrumentName:
                arrangeCount += measurement;
                break;
        }
    }

    private void UpdateTotals(ProcessStatistics statistics)
    {
        var snapshot = Snapshot;
        snapshot.Time = DateTime.Now;
        snapshot.ThreadCount = statistics.ThreadCount;
        snapshot.ThreadLevel = ThreadLevelOf(statistics.ThreadCount);
        snapshot.WorkingSet = statistics.WorkingSet;
        snapshot.MemoryLevel = MemoryLevelOf(statistics.WorkingSet);
        snapshot.ManagedMemory = GC.GetTotalMemory(false);
        snapshot.Gc0Count = statistics.Gc0Count;
        snapshot.Gc1Count = statistics.Gc1Count;
        snapshot.Gc2Count = statistics.Gc2Count;
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    private static DiagnosticLevel FpsLevelOf(double fps) => fps switch
    {
        >= 50 => DiagnosticLevel.Safe,
        >= 30 => DiagnosticLevel.Warning,
        _ => DiagnosticLevel.Critical
    };

    private static DiagnosticLevel CpuLevelOf(double usage) => usage switch
    {
        <= 30 => DiagnosticLevel.Safe,
        <= 60 => DiagnosticLevel.Warning,
        _ => DiagnosticLevel.Critical
    };

    private static DiagnosticLevel ThreadLevelOf(int count) => count switch
    {
        <= 64 => DiagnosticLevel.Safe,
        <= 128 => DiagnosticLevel.Warning,
        _ => DiagnosticLevel.Critical
    };

    private static DiagnosticLevel MemoryLevelOf(long workingSet) => (workingSet / MegaByte) switch
    {
        <= 256 => DiagnosticLevel.Safe,
        <= 512 => DiagnosticLevel.Warning,
        _ => DiagnosticLevel.Critical
    };

    private static DiagnosticLevel AllocationLevelOf(double bytesPerSecond) => (bytesPerSecond / MegaByte) switch
    {
        <= 4 => DiagnosticLevel.Safe,
        <= 8 => DiagnosticLevel.Warning,
        _ => DiagnosticLevel.Critical
    };

    private static DiagnosticLevel LayoutLevelOf(long count) => count switch
    {
        0 => DiagnosticLevel.Safe,
        <= 500 => DiagnosticLevel.Warning,
        _ => DiagnosticLevel.Critical
    };

    private static DiagnosticLevel BatteryLevelOf(double? charge) => charge switch
    {
        null => DiagnosticLevel.Warning,
        > 0.5 => DiagnosticLevel.Safe,
        > 0.2 => DiagnosticLevel.Warning,
        _ => DiagnosticLevel.Critical
    };

    // Not connected to WiFi is a warning
    private static DiagnosticLevel WiFiLevelOf(int? signalStrength) => signalStrength switch
    {
        null => DiagnosticLevel.Warning,
        >= -67 => DiagnosticLevel.Safe,
        >= -80 => DiagnosticLevel.Warning,
        _ => DiagnosticLevel.Critical
    };
}
