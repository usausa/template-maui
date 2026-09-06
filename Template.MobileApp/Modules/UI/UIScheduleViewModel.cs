namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Models.Sample.Calendar;
using Template.MobileApp.Services;

public sealed partial class UIScheduleViewModel : AppViewModelBase
{
    private readonly IScheduleEventProvider scheduleService;

    private readonly IDispatcherTimer timer;

    [ObservableProperty]
    public partial IReadOnlyList<TimetableDay> Days { get; private set; } = [];

    [ObservableProperty]
    public partial TimetableDay? SelectedDay { get; set; }

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

    public UIScheduleViewModel(
        IDispatcher dispatcher,
        IScheduleEventProvider scheduleService)
    {
        this.scheduleService = scheduleService;

        // 現在時刻ラインは 1 分毎に更新する
        CurrentTime = DateTime.Now.TimeOfDay;
        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMinutes(1);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => CurrentTime = DateTime.Now.TimeOfDay));

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectedDay))
            {
                UpdateEvents();
            }
        };
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (Days.Count == 0)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            Days = Enumerable.Range(0, 7).Select(x => new TimetableDay { Date = today.AddDays(x) }).ToList();
            SelectedDay = Days[0];
        }

        CurrentTime = DateTime.Now.TimeOfDay;
        timer.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    private void UpdateEvents()
    {
        var day = SelectedDay;
        if (day is null)
        {
            Events = [];
            ShowCurrentTime = false;
            UpdateSummary();
            return;
        }

        // 日付単位のサンプルイベントへ決定論的に時間帯を割り当ててタイムテーブル化する
        var source = scheduleService.GetEvents(day.Date, day.Date);
        var events = new List<TimetableEvent>(source.Count);
        var index = 0;
        foreach (var ev in source)
        {
            var startHour = 8 + ((StableHash(ev.Id) + (index * 3)) % 9);
            var duration = 1 + (StableHash(ev.Title) % 2);
            var color = ev.Style == ScheduleStyle.Filled ? ev.BackgroundColor : ev.TextColor;
            events.Add(new TimetableEvent
            {
                Title = ev.Title,
                Start = TimeSpan.FromHours(startHour),
                End = TimeSpan.FromHours(Math.Min(startHour + duration, 20)),
                Color = color
            });
            index++;
        }

        Events = events;
        ShowCurrentTime = day.IsToday;
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        EventCount = Events.Count;
        TotalTimeText = TimetableCalculator.FormatDuration(TimeSpan.FromTicks(Events.Sum(static x => (x.End - x.Start).Ticks)));

        // 空き時間は 8:00-20:00 のうちイベントで埋まっていない時間 (重複はマージして数える)
        var busy = TimetableCalculator.MergeBusy(Events, TimeSpan.FromHours(8), TimeSpan.FromHours(20));
        var busyTotal = TimeSpan.FromTicks(busy.Sum(static x => (x.End - x.Start).Ticks));
        FreeTimeText = TimetableCalculator.FormatDuration(TimeSpan.FromHours(12) - busyTotal);
    }

    private static int StableHash(string value)
    {
        var hash = 0;
        foreach (var c in value)
        {
            hash = ((hash * 31) + c) & 0x7FFFFFFF;
        }
        return hash;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
