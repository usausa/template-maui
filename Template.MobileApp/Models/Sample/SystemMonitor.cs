namespace Template.MobileApp.Models.Sample;

using ClamChart;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class MonitorSample
{
    public static ChartAccessorCollection<MonitorSample> Accessors { get; } = new()
    {
        { "time", static x => x.Time },
        { "core1", static x => x.Core1 },
        { "core2", static x => x.Core2 },
        { "core3", static x => x.Core3 },
        { "core4", static x => x.Core4 },
        { "cpu", static x => x.CpuTemperature },
        { "gpu", static x => x.GpuTemperature },
        { "receive", static x => x.Receive },
        { "send", static x => x.Send },
        { "queue", static x => x.Queue }
    };

    public required DateTime Time { get; init; }

    public required double Core1 { get; init; }

    public required double Core2 { get; init; }

    public required double Core3 { get; init; }

    public required double Core4 { get; init; }

    public required double CpuTemperature { get; init; }

    public required double GpuTemperature { get; init; }

    public required double Receive { get; init; }

    public required double Send { get; init; }

    public required double Queue { get; init; }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// A fake machine, the numbers wander within a band instead of being random noise
public sealed class SystemMonitor
{
    private const int LoadCount = 4;

    private readonly double[] loads = [28, 21, 34, 17];

    private double gpuTemperature = 46;

    private double receive = 220;

    private double send = 90;

    private int step;

    public double Cpu { get; private set; }

    public double Memory { get; private set; } = 53;

    public double Temperature { get; private set; } = 54;

    public double Network => receive + send;

    public MonitorSample Next(DateTime time)
    {
        step++;

        for (var i = 0; i < LoadCount; i++)
        {
            var wave = Math.Sin((step * 0.11) + (i * 1.3)) * 14;
            var drift = ((step * (7 + i)) % 13) - 6;
            loads[i] = Math.Clamp(((loads[i] * 2) + 30 + wave + drift) / 3, 2, 99);
        }

        Cpu = loads.Average();
        Memory = Math.Clamp(Memory + ((((step * 5) % 7) - 3) * 0.15), 38, 72);
        Temperature = Math.Clamp(Temperature + ((Cpu - 35) * 0.04) + ((((step * 3) % 5) - 2) * 0.2), 40, 92);
        gpuTemperature = Math.Clamp(gpuTemperature + ((((step * 11) % 7) - 3) * 0.25), 35, 80);
        receive = Math.Clamp((receive * 0.7) + (Math.Abs(Math.Sin(step * 0.17)) * 420) + (((step * 13) % 17) * 3), 5, 1200);
        send = Math.Clamp((send * 0.7) + (Math.Abs(Math.Cos(step * 0.23)) * 160) + (((step * 7) % 11) * 2), 2, 600);

        return new MonitorSample
        {
            Time = time,
            Core1 = loads[0],
            Core2 = loads[1],
            Core3 = loads[2],
            Core4 = loads[3],
            CpuTemperature = Temperature,
            GpuTemperature = gpuTemperature,
            Receive = receive,
            Send = send,
            Queue = Math.Round(Math.Max(0, (Cpu / 18) + (((step * 5) % 7) - 2)))
        };
    }

    public static IReadOnlyList<ChartEntry> LoadDisks() =>
    [
        new("C: システム", 57, null, 100),
        new("D: データ", 23, null, 100),
        new("E: 作業", 67, null, 100),
        new("F: 退避", 82, null, 100)
    ];
}
