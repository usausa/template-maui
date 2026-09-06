namespace Template.MobileApp.Modules.UI;

using System.Collections.ObjectModel;
using System.Globalization;

using Template.MobileApp.Models.Sample.Calendar;
using Template.MobileApp.Services;

public sealed partial class UICalendarViewModel : AppViewModelBase
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private readonly IDialog dialog;

    private readonly IScheduleEventProvider scheduleService;
    private readonly HolidayService holidayService;

    private MonthViewBuilder builder = new();
    private int currentYear;
    private int currentMonth;

    [ObservableProperty]
    public partial MonthView? MonthView { get; private set; }

#pragma warning disable SA1500
    public DayOfWeek FirstDayOfWeek
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }
            field = value;

            RaisePropertyChanged(new PropertyChangedEventArgs(nameof(FirstDayOfWeek)));

            builder = new MonthViewBuilder(value);
            LoadMonth(currentYear, currentMonth);
        }
    } = DayOfWeek.Monday;
#pragma warning restore SA1500

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
    public partial DateOnly? MinDate { get; set; }

    [ObservableProperty]
    public partial DateOnly? MaxDate { get; set; }

    [ObservableProperty]
    public partial CultureInfo? Culture { get; set; }

    public IObserveCommand PrevMonthCommand { get; }
    public IObserveCommand NextMonthCommand { get; }
    public IObserveCommand GoToTodayCommand { get; }
    public IObserveCommand DayTappedCommand { get; }
    public IObserveCommand EventTappedCommand { get; }
    public IObserveCommand SelectModeCommand { get; }

    public UICalendarViewModel(
        IDialog dialog,
        IScheduleEventProvider scheduleService,
        HolidayService holidayService)
    {
        this.dialog = dialog;
        this.scheduleService = scheduleService;
        this.holidayService = holidayService;

        PrevMonthCommand = MakeDelegateCommand(OnPrevMonth);
        NextMonthCommand = MakeDelegateCommand(OnNextMonth);
        GoToTodayCommand = MakeDelegateCommand(OnGoToToday);
        DayTappedCommand = MakeAsyncCommand<DayView>(OnDayTappedAsync);
        EventTappedCommand = MakeAsyncCommand<ScheduleEvent>(OnEventTappedAsync);
        SelectModeCommand = MakeDelegateCommand<CalendarSelectionMode>(OnSelectMode);

        currentYear = Today.Year;
        currentMonth = Today.Month;
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (MonthView is null)
        {
            LoadMonth(currentYear, currentMonth);
        }
        return Task.CompletedTask;
    }

    private void OnPrevMonth()
    {
        var prev = new DateOnly(currentYear, currentMonth, 1).AddMonths(-1);
        currentYear = prev.Year;
        currentMonth = prev.Month;
        LoadMonth(currentYear, currentMonth);
    }

    private void OnNextMonth()
    {
        var next = new DateOnly(currentYear, currentMonth, 1).AddMonths(1);
        currentYear = next.Year;
        currentMonth = next.Month;
        LoadMonth(currentYear, currentMonth);
    }

    private void OnGoToToday()
    {
        currentYear = Today.Year;
        currentMonth = Today.Month;
        LoadMonth(currentYear, currentMonth);
    }

    private void OnSelectMode(CalendarSelectionMode mode)
    {
        SelectionMode = mode;

        // モード切替時は選択状態をリセットする
        SelectedDate = null;
        SelectedDates.Clear();
        SelectedStartDate = null;
        SelectedEndDate = null;
    }

    private void LoadMonth(int year, int month)
    {
        var (rangeStart, rangeEnd) = builder.GetDisplayRange(year, month);
        var events = scheduleService.GetEvents(rangeStart, rangeEnd);
        var stamps = scheduleService.GetStamps(rangeStart, rangeEnd);
        var holidays = holidayService.GetHolidays(rangeStart, rangeEnd);

        MonthView = builder.Build(year, month, Today, events, stamps, holidays);
    }

    private async Task OnDayTappedAsync(DayView? day)
    {
        if (day is not null)
        {
            await dialog.Toast($"{day.Date:yyyy/MM/dd}");
        }
    }

    private async Task OnEventTappedAsync(ScheduleEvent? evt)
    {
        if (evt is not null)
        {
            await dialog.Toast(evt.Title);
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
