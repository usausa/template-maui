namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

public sealed partial class UITransitTimetableViewModel : AppViewModelBase
{
    // Field

    private readonly TimeProvider timeProvider;

    // Property

    [Scope]
    [ObservableProperty]
    public partial UITransitContext Context { get; set; } = default!;

    public TimetableDrawing Drawing { get; } = new();

    [ObservableProperty]
    public partial StationTimetable Timetable { get; set; } = default!;

    [ObservableProperty]
    public partial StationDirection Direction { get; set; }

    [ObservableProperty]
    public partial StationDayType DayType { get; set; }

    [ObservableProperty]
    public partial TimeOnly Now { get; set; }

    public ScrollController Scroller { get; } = new();

    // Command

    public IObserveCommand DirectionCommand { get; }

    public IObserveCommand DayTypeCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UITransitTimetableViewModel(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        DirectionCommand = MakeDelegateCommand<StationDirection>(x =>
        {
            Direction = x;
            Reload();
        });
        DayTypeCommand = MakeDelegateCommand<StationDayType>(x =>
        {
            DayType = x;
            Reload();
        });
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Direction = Context.Direction;
            DayType = Context.DayType;
            Reload();
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.PopAsync();

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Scroller.ScrollTo(0, Drawing.OffsetOf(Now.Hour));
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Reload()
    {
        Now = TimeOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        Timetable = StationTimetableSample.Load(Direction, DayType);

        Drawing.Update(Timetable, Now);
    }
}
