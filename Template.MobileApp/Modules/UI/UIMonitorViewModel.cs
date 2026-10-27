namespace Template.MobileApp.Modules.UI;

using ClamChart;

public sealed partial class UIMonitorViewModel : AppViewModelBase
{
    // Field

    private const int HistoryCount = 190;

    private const int TrendCount = 30;

    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    private static readonly TimeSpan LongWindow = TimeSpan.FromMinutes(3);

    private static readonly TimeSpan ShortWindow = TimeSpan.FromSeconds(45);

    private readonly TimeProvider timeProvider;

    private readonly IDispatcherTimer timer;

    private readonly SystemMonitor monitor = new();

    // Property

    public ObservableCollection<MonitorSample> Samples { get; } = [];

    public ObservableCollection<double> CpuTrend { get; } = [];

    public ObservableCollection<double> MemoryTrend { get; } = [];

    public ObservableCollection<double> TemperatureTrend { get; } = [];

    public ObservableCollection<double> NetworkTrend { get; } = [];

    public IReadOnlyList<ChartEntry> Disks { get; } = SystemMonitor.LoadDisks();

    [ObservableProperty]
    public partial double Cpu { get; set; }

    [ObservableProperty]
    public partial double Memory { get; set; }

    [ObservableProperty]
    public partial double Temperature { get; set; }

    [ObservableProperty]
    public partial double Network { get; set; }

    [ObservableProperty]
    public partial TimeSpan Window { get; set; } = LongWindow;

    [ObservableProperty]
    public partial bool IsWide { get; set; } = true;

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIMonitorViewModel(TimeProvider timeProvider, IDispatcher dispatcher)
    {
        this.timeProvider = timeProvider;

        timer = dispatcher.CreateTimer();
        timer.Interval = Interval;
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => Tick()));

        var time = timeProvider.GetLocalNow().DateTime - (Interval * (HistoryCount - 1));
        for (var i = 0; i < HistoryCount; i++)
        {
            Add(monitor.Next(time));
            time += Interval;
        }
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        timer.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu3);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        IsWide = !IsWide;
        Window = IsWide ? LongWindow : ShortWindow;
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Tick() => Add(monitor.Next(timeProvider.GetLocalNow().DateTime));

    private void Add(MonitorSample sample)
    {
        Samples.Add(sample);
        if (Samples.Count > HistoryCount)
        {
            Samples.RemoveAt(0);
        }

        Cpu = monitor.Cpu;
        Memory = monitor.Memory;
        Temperature = monitor.Temperature;
        Network = monitor.Network;

        Push(CpuTrend, Cpu);
        Push(MemoryTrend, Memory);
        Push(TemperatureTrend, Temperature);
        Push(NetworkTrend, Network);
    }

    private static void Push(ObservableCollection<double> values, double value)
    {
        values.Add(value);
        if (values.Count > TrendCount)
        {
            values.RemoveAt(0);
        }
    }
}
