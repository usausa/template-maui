namespace Template.MobileApp.Modules.Network;

public sealed class NetworkRealtimeViewModel : AppViewModelBase
{
    private readonly IDispatcherTimer timer;

    private int counter;

    public StatDataSet CpuLoadSet { get; }

    public NetworkRealtimeViewModel()
    {
        CpuLoadSet = new StatDataSet(101);

        // TODO
        timer = Application.Current?.Dispatcher.CreateTimer()!;
        timer.Interval = TimeSpan.FromSeconds(1);
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
        CpuLoadSet.Add(counter);

        counter++;
        if (counter > 100)
        {
            counter = 0;
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
