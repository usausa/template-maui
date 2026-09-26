namespace Template.MobileApp.Components;

using System.Diagnostics;

public sealed record BatteryStatus(
    double Level,
    BatteryState State,
    BatteryPowerSource PowerSource);

public sealed record NetworkStatus(
    NetworkAccess Access,
    IReadOnlyList<ConnectionProfile> Profiles);

public sealed record WiFiStatus(
    int SignalStrength,
    int LinkSpeed);

public readonly record struct ProcessStatistics(
    long Timestamp,
    TimeSpan CpuTime,
    long WorkingSet,
    int ThreadCount,
    int Gc0Count,
    int Gc1Count,
    int Gc2Count,
    long AllocatedBytes);

public sealed partial class DeviceInformation : IDisposable
{
    private bool started;

    // ------------------------------------------------------------
    // Control
    // ------------------------------------------------------------

    public void Start()
    {
        if (started)
        {
            return;
        }

        started = true;
        StartBattery();
        StartNetwork();
    }

    public void Stop()
    {
        if (!started)
        {
            return;
        }

        started = false;
        StopNetwork();
        StopBattery();
    }

    // ------------------------------------------------------------
    // Identity
    // ------------------------------------------------------------

    public string DeviceId { get; } = ResolveDeviceId();

    private static partial string ResolveDeviceId();

    // ------------------------------------------------------------
    // Battery
    // ------------------------------------------------------------

    public event EventHandler? BatteryChanged;

    public BatteryStatus? Battery { get; private set; }

    private void UpdateBattery(BatteryStatus status)
    {
        Battery = status;
        BatteryChanged?.Invoke(this, EventArgs.Empty);
    }

    private partial void StartBattery();

    private partial void StopBattery();

    // ------------------------------------------------------------
    // Network
    // ------------------------------------------------------------

    public event EventHandler? NetworkChanged;

    public event EventHandler? WiFiChanged;

    public NetworkStatus? Network { get; private set; }

    public WiFiStatus? WiFi { get; private set; }

    private void UpdateNetwork(NetworkStatus status)
    {
        Network = status;
        NetworkChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateWiFi(WiFiStatus? status)
    {
        WiFi = status;
        WiFiChanged?.Invoke(this, EventArgs.Empty);
    }

    private partial void StartNetwork();

    private partial void StopNetwork();

    // ------------------------------------------------------------
    // Process
    // ------------------------------------------------------------

    public DateTime StartTime { get; } = ReadStartTime();

    public ProcessStatistics ReadProcessStatistics()
    {
        ReadProcessStat(out var threadCount, out var workingSet);
        return new ProcessStatistics(
            Stopwatch.GetTimestamp(),
            Environment.CpuUsage.TotalTime,
            workingSet,
            threadCount,
            GC.CollectionCount(0),
            GC.CollectionCount(1),
            GC.CollectionCount(2),
            GC.GetTotalAllocatedBytes());
    }

    public static long ReadHeapSize() => GC.GetGCMemoryInfo().HeapSizeBytes;

    private static DateTime ReadStartTime()
    {
        using var process = Process.GetCurrentProcess();
        return process.StartTime;
    }

    private partial void ReadProcessStat(out int threadCount, out long workingSet);
}
