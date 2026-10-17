namespace Template.MobileApp.Modules.UI;

using System.Collections.ObjectModel;

using ClamCalendar;

using Template.MobileApp.Services;

public sealed partial class UICalendarViewModel : AppViewModelBase
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private readonly ICalendarService calendarService;

    [ObservableProperty]
    public partial DateOnly DisplayDate { get; set; } = Today;

    [ObservableProperty]
    public partial IReadOnlyList<CalendarEvent> Events { get; private set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<CalendarStamp> Stamps { get; private set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> Holidays { get; private set; } = [];

    [ObservableProperty]
    public partial CalendarSelectionMode SelectionMode { get; set; } = CalendarSelectionMode.None;

    [ObservableProperty]
    public partial DateOnly? SelectedDate { get; set; }

    public ObservableCollection<DateOnly> SelectedDates { get; } = [];

    [ObservableProperty]
    public partial DateOnly? SelectedStartDate { get; set; }

    [ObservableProperty]
    public partial DateOnly? SelectedEndDate { get; set; }

    [ObservableProperty]
    public partial bool IsSettingOpen { get; set; }

    public IObserveCommand DisplayDateChangedCommand { get; }
    public IObserveCommand DayTappedCommand { get; }
    public IObserveCommand EventTappedCommand { get; }
    public IObserveCommand SelectModeCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UICalendarViewModel(
        IDialog dialog,
        ICalendarService calendarService)
    {
        this.calendarService = calendarService;

        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(CommandMode.Simple, Load);
        DayTappedCommand = MakeAsyncCommand<CalendarDayEventArgs>(x => dialog.Toast($"{x.Date:yyyy/MM/dd}").AsTask());
        EventTappedCommand = MakeAsyncCommand<CalendarEventEventArgs>(x => dialog.Toast(x.Event.Title).AsTask());
        SelectModeCommand = MakeDelegateCommand<CalendarSelectionMode>(SelectMode);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync()
    {
        if (IsSettingOpen)
        {
            IsSettingOpen = false;
            return Task.CompletedTask;
        }

        return Navigator.ForwardAsync(ViewId.UIMenu1);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        IsSettingOpen = !IsSettingOpen;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        DisplayDate = Today;
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Load(CalendarDisplayDateChangedEventArgs e)
    {
        Events = calendarService.GetEvents(e.FirstDate, e.LastDate);
        Stamps = calendarService.GetStamps(e.FirstDate, e.LastDate);
        Holidays = calendarService.GetHolidays(e.FirstDate, e.LastDate);
    }

    private void SelectMode(CalendarSelectionMode mode)
    {
        SelectionMode = mode;

        SelectedDate = null;
        SelectedDates.Clear();
        SelectedStartDate = null;
        SelectedEndDate = null;

        IsSettingOpen = false;
    }
}
