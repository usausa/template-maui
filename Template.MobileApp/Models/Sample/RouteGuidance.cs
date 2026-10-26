namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum TurnKind
{
    Straight,
    Left,
    Right,
    SlightLeft,
    SlightRight,
    Merge,
    Arrive
}

[Flags]
public enum LaneArrow
{
    None = 0,
    Left = 1,
    Straight = 2,
    Right = 4
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record GuideStep(TurnKind Turn, int Distance, string Road, string Spot);

public sealed record LaneGuide(LaneArrow Arrows, bool IsRecommended);

// A road segment of the schematic map, in map units
public sealed record NaviRoad(float X1, float Y1, float X2, float Y2, float Width, string Name);

public sealed record NaviPoi(float X, float Y, string Name, string Category);

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed partial class RouteGuidance : ObservableObject
{
    private const int RoundUnit = 50;

    public required IReadOnlyList<GuideStep> Steps { get; init; }

    public required IReadOnlyList<LaneGuide> Lanes { get; init; }

    public required IReadOnlyList<NaviRoad> Roads { get; init; }

    public required IReadOnlyList<NaviPoi> Spots { get; init; }

    public required IReadOnlyList<PointF> Route { get; init; }

    public required string Destination { get; init; }

    [ObservableProperty(NotifyAlso = [nameof(Step), nameof(NextStep), nameof(TurnProgress)])]
    public partial int StepIndex { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(RoundedStepRemain), nameof(TurnProgress)])]
    public partial int StepRemain { get; set; }

    [ObservableProperty]
    public partial int TotalRemain { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(Eta)])]
    public partial int RemainMinutes { get; set; }

    [ObservableProperty]
    public partial int Speed { get; set; }

    [ObservableProperty]
    public partial int SpeedLimit { get; set; }

    [ObservableProperty]
    public partial bool IsMuted { get; set; }

    public GuideStep Step => Steps[Math.Clamp(StepIndex, 0, Steps.Count - 1)];

    public GuideStep? NextStep => StepIndex + 1 < Steps.Count ? Steps[StepIndex + 1] : null;

    public int RoundedStepRemain => StepRemain / RoundUnit * RoundUnit;

    public double TurnProgress => Step.Distance > 0
        ? Math.Clamp(1d - ((double)StepRemain / Step.Distance), 0d, 1d)
        : 0d;

    public DateTime Eta => DateTime.Now.AddMinutes(RemainMinutes);

    public void Advance(int meters)
    {
        TotalRemain = Math.Max(0, TotalRemain - meters);
        RemainMinutes = Math.Max(1, (int)Math.Ceiling(TotalRemain / 650d));

        if (TotalRemain == 0)
        {
            StepRemain = 0;
            return;
        }

        var remain = StepRemain - meters;
        if (remain > 0)
        {
            StepRemain = remain;
            return;
        }

        if (StepIndex + 1 < Steps.Count)
        {
            StepIndex++;
            StepRemain = Step.Distance;
        }
        else
        {
            StepRemain = 0;
        }
    }

    public double RouteProgress() =>
        Steps.Count > 0 ? Math.Clamp((StepIndex + TurnProgress) / Steps.Count, 0d, 1d) : 0d;
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class NaviSample
{
    private static readonly GuideStep[] Plan =
    [
        new(TurnKind.Right, 300, "県道 15 号", "勝川橋西 交差点"),
        new(TurnKind.Merge, 1200, "国道 19 号", "勝川 IC 方面"),
        new(TurnKind.SlightLeft, 2400, "国道 19 号", "大曽根 分岐"),
        new(TurnKind.Left, 1800, "市道 環状線", "矢田川 北"),
        new(TurnKind.Right, 900, "県道 30 号", "東片端 交差点"),
        new(TurnKind.Arrive, 400, "目的地", "名古屋城 北駐車場")
    ];

    private static readonly LaneGuide[] LanePlan =
    [
        new(LaneArrow.Left, false),
        new(LaneArrow.Straight, false),
        new(LaneArrow.Straight, true),
        new(LaneArrow.Straight | LaneArrow.Right, true),
        new(LaneArrow.Right, true)
    ];

    private static readonly NaviRoad[] RoadPlan =
    [
        new(0, 22, 100, 18, 1.7f, "Rt.15"),
        new(0, 58, 100, 62, 2.2f, "Rt.19"),
        new(0, 86, 100, 88, 1.2f, string.Empty),
        new(18, 0, 22, 100, 1.5f, "Loop"),
        new(52, 0, 48, 100, 1.9f, string.Empty),
        new(80, 0, 84, 100, 1.2f, "Rt.30")
    ];

    private static readonly NaviPoi[] SpotPlan =
    [
        new(30, 32, "ENEOS", "fuel"),
        new(62, 46, "Kachigawa Sta.", "station"),
        new(74, 72, "FamilyMart", "store"),
        new(14, 68, "Kachigawa El.", "school")
    ];

    private static readonly PointF[] RoutePlan =
    [
        new(10f, 86.2f), new(21.46f, 86.43f), new(20.35f, 58.81f), new(49.6f, 59.98f),
        new(51.2f, 19.95f), new(80.75f, 18.77f), new(80.24f, 6f)
    ];

    public static RouteGuidance LoadGuidance() =>
        new()
        {
            Steps = Plan,
            Lanes = LanePlan,
            Roads = RoadPlan,
            Spots = SpotPlan,
            Route = RoutePlan,
            Destination = "名古屋城 北駐車場",
            StepIndex = 0,
            StepRemain = Plan[0].Distance,
            TotalRemain = Plan.Sum(static x => x.Distance),
            RemainMinutes = 11,
            Speed = 48,
            SpeedLimit = 50
        };
}
