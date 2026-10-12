namespace Template.MobileApp.Modules.UI;

public sealed class UIKitTrackingStep
{
    public string Title { get; init; } = string.Empty;
    public DateTime Time { get; init; }
    public bool Done { get; init; }
    public bool Current { get; init; }
    public int Delay { get; init; }

    // 済んだ段階の線を伸ばし始める時間 (ms)。上から順に伸ばす
    public int LineDelay { get; init; }
    public bool IsComplete => Done;
    public bool IsCurrent => Current;
    public bool IsPending => !Done && !Current;
}

public sealed partial class UIKitTrackingViewModel : AppViewModelBase
{
    private readonly IDispatcherTimer timer;

    public string OrderNumber { get; }

    public DateTime Eta { get; }

    public IReadOnlyList<UIKitTrackingStep> Steps { get; }

    // 到着までの分
    [ObservableProperty]
    public partial int RemainMinutes { get; set; }

    // 配達を始めてから到着の予定までの進み具合
    [ObservableProperty]
    public partial double DeliveryProgress { get; set; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIKitTrackingViewModel(IDispatcher dispatcher)
    {
        var activity = KitSample.LoadActivity(DateTime.Now);
        OrderNumber = activity.OrderNumber;
        Eta = activity.OrderEta;
        Steps =
        [
            new() { Title = "注文受付", Time = Eta.Date.AddDays(-1).AddHours(19).AddMinutes(40), Done = true, Delay = 0, LineDelay = 400 },
            new() { Title = "梱包完了", Time = Eta.Date.AddDays(-1).AddHours(21).AddMinutes(15), Done = true, Delay = 80, LineDelay = 800 },
            new() { Title = "発送済み", Time = Eta.AddHours(-3), Done = true, Delay = 160, LineDelay = 1200 },
            new() { Title = "配達中", Time = Eta.AddMinutes(-70), Current = true, Delay = 240 },
            new() { Title = "配達完了", Time = Eta, Delay = 320 }
        ];

        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => UpdateRemain()));
        UpdateRemain();
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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIKitDash);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void UpdateRemain()
    {
        var now = DateTime.Now;
        var start = Steps[3].Time;
        RemainMinutes = Eta > now ? (int)Math.Ceiling((Eta - now).TotalMinutes) : 0;
        DeliveryProgress = Math.Clamp((now - start).TotalMinutes / (Eta - start).TotalMinutes, 0d, 1d);
    }
}
