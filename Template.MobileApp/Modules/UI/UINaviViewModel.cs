namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Scene;

public sealed class UINaviLane : ObservableObject
{
    public required LaneGuide Source { get; init; }

    public bool IsRecommended => Source.IsRecommended;

    public bool HasLeft => Source.Arrows.HasFlag(LaneArrow.Left);

    public bool HasStraight => Source.Arrows.HasFlag(LaneArrow.Straight);

    public bool HasRight => Source.Arrows.HasFlag(LaneArrow.Right);
}

public sealed partial class UINaviViewModel : AppViewModelBase
{
    // Field

    private const int MetersPerTick = 25;

    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(250);

    private static readonly TimeSpan FastInterval = TimeSpan.FromMilliseconds(80);

    private static readonly int[] SpeedPattern = [48, 51, 46, 43, 49, 54, 50, 44];

    private readonly IDispatcherTimer timer;

    private int tick;

    // Property

    [ObservableProperty]
    public partial RouteGuidance Guidance { get; set; } = NaviSample.LoadGuidance();

    public NaviScene Scene { get; } = new();

    public IReadOnlyList<UINaviLane> Lanes { get; }

    [ObservableProperty]
    public partial bool IsFast { get; set; }

    // Command

    public IObserveCommand FinishCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UINaviViewModel(IDispatcher dispatcher)
    {
        Lanes = [.. Guidance.Lanes.Select(x => new UINaviLane { Source = x })];

        FinishCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIMenu2));

        Scene.Update(Guidance);
        Disposables.Add(Scene);

        timer = dispatcher.CreateTimer();
        timer.Interval = Interval;
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => Tick()));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        Scene.Start();
        timer.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        Scene.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        IsFast = !IsFast;
        timer.Interval = IsFast ? FastInterval : Interval;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        Reset();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Reset()
    {
        tick = 0;
        Guidance = NaviSample.LoadGuidance();
        Scene.Update(Guidance);
        timer.Start();
    }

    private void Tick() => Move(MetersPerTick);

    private void Move(int meters)
    {
        Guidance.Advance(meters);
        if (Guidance.TotalRemain == 0)
        {
            Guidance.Speed = 0;
            timer.Stop();
            return;
        }

        Guidance.Speed = SpeedPattern[tick % SpeedPattern.Length];
        tick++;
    }
}
