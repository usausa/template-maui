namespace Template.MobileApp.Modules.UI;

using System.Collections.ObjectModel;

using ClamCalendar;

using Template.MobileApp.Services;

using CommandBehavior = Smart.Maui.ViewModels.CommandBehavior;

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

    public IObserveCommand DisplayDateChangedCommand { get; }
    public IObserveCommand GoToTodayCommand { get; }
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

        // 月が変わるたびに表示範囲が通知される (初回はナビゲーション中に来るので Busy でも実行する)
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(Load, CommandBehavior.AllowBusyExecution);
        GoToTodayCommand = MakeDelegateCommand(() => DisplayDate = Today);
        DayTappedCommand = MakeAsyncCommand<CalendarDayEventArgs>(x => dialog.Toast($"{x.Date:yyyy/MM/dd}").AsTask());
        EventTappedCommand = MakeAsyncCommand<CalendarEventEventArgs>(x => dialog.Toast(x.Event.Title).AsTask());
        SelectModeCommand = MakeDelegateCommand<CalendarSelectionMode>(OnSelectMode);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Load(CalendarDisplayDateChangedEventArgs e)
    {
        Events = calendarService.GetEvents(e.FirstDate, e.LastDate);
        Stamps = calendarService.GetStamps(e.FirstDate, e.LastDate);
        Holidays = calendarService.GetHolidays(e.FirstDate, e.LastDate);
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
}
