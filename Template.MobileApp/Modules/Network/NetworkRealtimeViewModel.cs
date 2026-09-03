namespace Template.MobileApp.Modules.Network;

#pragma warning disable CA5394
public sealed class NetworkRealtimeViewModel : AppViewModelBase
{
    private readonly IDispatcherTimer timer;

    private readonly Random random = new();

    private double cpu = 35;

    private double memory = 55;

    private double network = 10;

    public StatDataSet CpuLoadSet { get; }

    public StatDataSet MemoryLoadSet { get; }

    public StatDataSet NetworkLoadSet { get; }

    public NetworkRealtimeViewModel()
    {
        CpuLoadSet = new StatDataSet(101);
        MemoryLoadSet = new StatDataSet(101);
        NetworkLoadSet = new StatDataSet(101);

        timer = Application.Current?.Dispatcher.CreateTimer()!;
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.Tick += OnTimerTick;
        timer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            timer.Stop();
            timer.Tick -= OnTimerTick;
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        // CPU: ゆらぎ+時折のスパイク
        cpu = Math.Clamp(cpu + (((random.NextDouble() * 2) - 1) * 8), 5, 65);
        if (random.NextDouble() < 0.05)
        {
            cpu = Math.Min(95, cpu + 30);
        }

        // Memory: 緩やかなドリフト
        memory = Math.Clamp(memory + (((random.NextDouble() * 2) - 1) * 2), 40, 85);

        // Network: バースト型(通常は減衰)
        network = random.NextDouble() < 0.12 ? 40 + (random.NextDouble() * 55) : Math.Max(3, network * 0.6);

        CpuLoadSet.Add((int)cpu);
        MemoryLoadSet.Add((int)memory);
        NetworkLoadSet.Add((int)network);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
