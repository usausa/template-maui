namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

public sealed partial class UIGanttViewModel : AppViewModelBase
{
    // Field

    private readonly TimeProvider timeProvider;

    // Property

    public GanttDrawing HeaderDrawing { get; } = new() { Part = GanttPart.Header };

    public GanttDrawing ChartDrawing { get; } = new() { Part = GanttPart.Chart };

    [ObservableProperty]
    public partial ProjectSchedule Schedule { get; set; }

    [ObservableProperty]
    public partial GanttScale Scale { get; set; } = GanttScale.Week;

    [ObservableProperty]
    public partial double ChartWidth { get; set; }

    [ObservableProperty]
    public partial double ChartHeight { get; set; }

    public ScrollController Scroller { get; } = new();

    // Command

    public IObserveCommand ScaleCommand { get; }

    public IObserveCommand TodayCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIGanttViewModel(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        ScaleCommand = MakeDelegateCommand<GanttScale>(x =>
        {
            Scale = x;
            Layout();
        });
        TodayCommand = MakeDelegateCommand(MoveToToday);

        Schedule = ProjectSample.LoadSchedule(DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
        Layout();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            MoveToToday();
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Schedule = ProjectSample.LoadSchedule(DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
        Layout();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Layout()
    {
        var width = ProjectSample.WidthOf(Scale);
        ChartWidth = Schedule.Days * width;
        ChartHeight = Schedule.TaskCount * ChartDrawing.RowHeight;

        HeaderDrawing.Update(Schedule, width);
        ChartDrawing.Update(Schedule, width);

        MoveToToday();
    }

    private void MoveToToday() =>
        Scroller.ScrollTo(Schedule.OffsetOf(Schedule.Today) * ProjectSample.WidthOf(Scale), 0);
}
