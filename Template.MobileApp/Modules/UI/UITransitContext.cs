namespace Template.MobileApp.Modules.UI;

public sealed partial class UITransitContext : ObservableObject
{
    private const int ShiftLimit = 4;

    [ObservableProperty]
    public partial IReadOnlyList<TransitRoute> Routes { get; set; }

    [ObservableProperty]
    public partial DateOnly Date { get; set; }

    [ObservableProperty]
    public partial bool IsReversed { get; set; }

    [ObservableProperty]
    public partial int Shift { get; set; }

    public StationDirection Direction { get; private set; }

    public StationDayType DayType { get; private set; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UITransitContext(TimeProvider timeProvider)
    {
        var now = timeProvider.GetLocalNow().DateTime;
        Date = DateOnly.FromDateTime(now);
        DayType = now.DayOfWeek switch
        {
            DayOfWeek.Saturday => StationDayType.Saturday,
            DayOfWeek.Sunday => StationDayType.Holiday,
            _ => StationDayType.Weekday
        };
        Routes = TransitSample.LoadRoutes(false, 0);
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    public bool CanShift(int offset) => Math.Abs(Shift + offset) <= ShiftLimit;

    public void Move(int offset)
    {
        Shift += offset;
        Reload();
    }

    public void Reverse()
    {
        IsReversed = !IsReversed;
        Shift = 0;
        Reload();
    }

    public void OpenTimetable(TransitRoute route) => Direction = route.TerminalDirection;

    private void Reload() => Routes = TransitSample.LoadRoutes(IsReversed, Shift);
}
