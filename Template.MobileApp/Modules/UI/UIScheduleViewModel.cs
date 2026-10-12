namespace Template.MobileApp.Modules.UI;

using ClamCalendar;

using Template.MobileApp.Services;

// 終日の予定 (カレンダーの予定の名前と色)
public sealed record UIScheduleAllDay(string Title, Color Color);

public sealed partial class UIScheduleViewModel : AppViewModelBase
{
    private static readonly TimeSpan StartTime = TimeSpan.FromHours(8);

    private static readonly TimeSpan EndTime = TimeSpan.FromHours(20);

    private readonly IDispatcherTimer timer;

    private readonly ICalendarService calendarService;

    [ObservableProperty]
    public partial IReadOnlyList<TimetableDay> Days { get; private set; } = [];

    [ObservableProperty]
    public partial TimetableDay? SelectedDay { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<UIScheduleAllDay> AllDayEvents { get; private set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<TimetableEvent> Events { get; private set; } = [];

    [ObservableProperty]
    public partial TimeSpan CurrentTime { get; set; }

    [ObservableProperty]
    public partial bool ShowCurrentTime { get; set; }

    // 日合計 (件数 / 所要時間合計 / 空き時間)
    [ObservableProperty]
    public partial int EventCount { get; private set; }

    [ObservableProperty]
    public partial string TotalTimeText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string FreeTimeText { get; private set; } = string.Empty;

    // 詳細のシートの予定 (開く前に設定する)
    [ObservableProperty]
    public partial TimetableEvent SelectedEvent { get; private set; } = default!;

    [ObservableProperty]
    public partial string SelectedTimeText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDurationText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsDetailOpen { get; set; }

    public IObserveCommand EventTappedCommand { get; }

    public IObserveCommand CloseDetailCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIScheduleViewModel(
        IDispatcher dispatcher,
        ICalendarService calendarService)
    {
        this.calendarService = calendarService;

        EventTappedCommand = MakeDelegateCommand<TimetableEvent>(x =>
        {
            SelectedEvent = x;
            SelectedTimeText = $"{x.Start:hh\\:mm} - {x.End:hh\\:mm}";
            SelectedDurationText = TimetableCalculator.FormatDuration(x.End - x.Start);
            IsDetailOpen = true;
        });
        CloseDetailCommand = MakeDelegateCommand(() => IsDetailOpen = false);

        // 現在時刻ラインは 1 分毎に更新する
        CurrentTime = DateTime.Now.TimeOfDay;
        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMinutes(1);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => CurrentTime = DateTime.Now.TimeOfDay));

        SubscribeSelectedDay(_ => UpdateEvents());
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            Days = Enumerable.Range(0, 7).Select(x => new TimetableDay { Date = today.AddDays(x) }).ToList();
            SelectedDay = Days[0];
        }

        CurrentTime = DateTime.Now.TimeOfDay;
        return Task.CompletedTask;
    }

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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void UpdateEvents()
    {
        var day = SelectedDay;
        if (day is null)
        {
            AllDayEvents = [];
            Events = [];
            ShowCurrentTime = false;
            UpdateSummary();
            return;
        }

        // 終日の予定はカレンダーと同じ予定、時刻のある予定はタイムテーブルに置く
        AllDayEvents = calendarService.GetEvents(day.Date, day.Date)
            .Select(static x => new UIScheduleAllDay(x.Title, x.Style == CalendarEventStyle.Filled ? x.BackgroundColor : x.TextColor))
            .ToArray();
        Events = calendarService.GetSchedule(day.Date);
        ShowCurrentTime = day.IsToday;
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        EventCount = Events.Count;
        TotalTimeText = TimetableCalculator.FormatDuration(TimeSpan.FromTicks(Events.Sum(static x => (x.End - x.Start).Ticks)));

        // 空き時間は表示範囲のうちイベントで埋まっていない時間
        FreeTimeText = TimetableCalculator.FormatDuration(EndTime - StartTime - TimetableCalculator.GetBusyTotal(Events, StartTime, EndTime));
    }
}
