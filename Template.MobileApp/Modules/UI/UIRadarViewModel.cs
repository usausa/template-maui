namespace Template.MobileApp.Modules.UI;

#pragma warning disable CA5394
public sealed class UIRadarViewModel : AppViewModelBase
{
    private readonly Random random = new();

    private readonly IDispatcherTimer timer;

    public ObservableCollection<RadarTarget> Targets { get; } = [];

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIRadarViewModel(IDispatcher dispatcher)
    {
        UpdateTargets();

        // 一定間隔でターゲットを入れ替えるデモ駆動
        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(5);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => UpdateTargets()));
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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void UpdateTargets()
    {
        Targets.Clear();
        Targets.AddRange(RadarSample.LoadTargets(random.NextDouble));
    }
}
#pragma warning restore CA5394
