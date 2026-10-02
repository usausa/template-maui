namespace Template.MobileApp.Modules.App;

using Template.MobileApp.Components;
using Template.MobileApp.Models.App;
using Template.MobileApp.Services;

public sealed partial class AppTimerViewModel : AppViewModelBase
{
    private const int NotificationId = 100;

    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(50);

    private static readonly TimeSpan FinishVibration = TimeSpan.FromSeconds(1);

    private readonly IScreen screen;

    private readonly IVibration vibration;

    private readonly IPopupNavigator popupNavigator;

    private readonly TimeProvider timeProvider;

    private readonly INotificationService notification;

    private readonly JsonStore store;

    private readonly Session session;

    private readonly IDispatcherTimer ticker;

    private readonly TimerStopwatch stopwatch = new();

    private readonly TimerCountdown countdown = new();

    private bool visible;

    [ObservableProperty]
    public partial TimerMode Mode { get; set; }

    // Stopwatch

    [ObservableProperty]
    public partial TimerStopwatchState StopwatchState { get; set; }

    [ObservableProperty]
    public partial TimeSpan Elapsed { get; set; }

    [ObservableProperty]
    public partial TimeSpan CurrentLap { get; set; }

    [ObservableProperty]
    public partial int LapNo { get; set; }

    // 1 分で 1 周
    [ObservableProperty]
    public partial double StopwatchProgress { get; set; }

    public TimerLapList Laps { get; } = [];

    // Countdown

    [ObservableProperty]
    public partial TimerCountdownState CountdownState { get; set; }

    [ObservableProperty]
    public partial TimeSpan Duration { get; set; }

    [ObservableProperty]
    public partial int DurationMinutes { get; set; }

    [ObservableProperty]
    public partial TimeSpan Remaining { get; set; }

    // 残りの割合 (減っていく)
    [ObservableProperty]
    public partial double CountdownProgress { get; set; }

    [ObservableProperty]
    public partial bool IsWarning { get; set; }

    // 計測中に終わる時刻 (端末の時刻)
    [ObservableProperty]
    public partial DateTime? EndTime { get; set; }

    public IObserveCommand BackCommand { get; }
    public IObserveCommand ModeCommand { get; }

    public IObserveCommand StartCommand { get; }
    public IObserveCommand PauseCommand { get; }
    public IObserveCommand LapCommand { get; }
    public IObserveCommand ResetCommand { get; }

    public IObserveCommand CountdownStartCommand { get; }
    public IObserveCommand CountdownPauseCommand { get; }
    public IObserveCommand CountdownResetCommand { get; }
    public IObserveCommand PresetCommand { get; }
    public IObserveCommand InputCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public AppTimerViewModel(
        IDispatcher dispatcher,
        IScreen screen,
        IVibration vibration,
        IPopupNavigator popupNavigator,
        TimeProvider timeProvider,
        INotificationService notification,
        JsonStore store,
        Session session)
    {
        this.screen = screen;
        this.vibration = vibration;
        this.popupNavigator = popupNavigator;
        this.timeProvider = timeProvider;
        this.notification = notification;
        this.store = store;
        this.session = session;

        ticker = dispatcher.CreateTimer();
        ticker.Interval = TickInterval;
        Disposables.Add(ticker.TickAsObservable().Subscribe(_ => Refresh()));
        Disposables.Add(new DelegateDisposable(ticker.Stop));

        // 背面では表示の更新を止め、前面に戻ったら今の時刻で計算し直す
        Disposables.Add(session.AsObservable(nameof(Session.IsForeground)).Subscribe(_ =>
        {
            Refresh();
            UpdateTicker();
        }));

        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
        ModeCommand = MakeDelegateCommand<TimerMode>(x => Mode = x);

        StartCommand = MakeDelegateCommand(StartStopwatch, () => StopwatchState != TimerStopwatchState.Running);
        PauseCommand = MakeDelegateCommand(PauseStopwatch, () => StopwatchState == TimerStopwatchState.Running);
        LapCommand = MakeDelegateCommand(TakeLap, () => StopwatchState == TimerStopwatchState.Running);
        ResetCommand = MakeDelegateCommand(ResetStopwatch, () => StopwatchState == TimerStopwatchState.Paused);

        CountdownStartCommand = MakeDelegateCommand(StartCountdown, () => (CountdownState != TimerCountdownState.Running) && (Duration > TimeSpan.Zero));
        CountdownPauseCommand = MakeDelegateCommand(PauseCountdown, () => CountdownState == TimerCountdownState.Running);
        CountdownResetCommand = MakeDelegateCommand(ResetCountdown, () => CountdownState is TimerCountdownState.Paused or TimerCountdownState.Finished);
        PresetCommand = MakeDelegateCommand<string>(x => SetDuration(Int32.Parse(x, CultureInfo.InvariantCulture)), _ => CountdownState != TimerCountdownState.Running);
        InputCommand = MakeAsyncCommand(InputDurationAsync, () => CountdownState != TimerCountdownState.Running);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Load();
            Laps.Rebuild(stopwatch.GetLaps());
            Refresh();
        }

        return Task.CompletedTask;
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        visible = true;
        UpdateTicker();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        visible = false;
        UpdateTicker();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.AppMenu);

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void StartStopwatch()
    {
        stopwatch.Start(timeProvider.GetUtcNow());
        ApplyStopwatch();
    }

    private void PauseStopwatch()
    {
        stopwatch.Pause(timeProvider.GetUtcNow());
        ApplyStopwatch();
    }

    private void TakeLap()
    {
        if (stopwatch.Lap(timeProvider.GetUtcNow()) is { } lap)
        {
            Laps.AddLatest(lap);
            Refresh();
            Save();
        }
    }

    private void ResetStopwatch()
    {
        stopwatch.Reset();
        Laps.Clear();
        ApplyStopwatch();
    }

    // 背面やアプリの終了の後でも、終わる時刻に通知で知らせる
    private void StartCountdown()
    {
        var now = timeProvider.GetUtcNow();
        countdown.Start(now);
        notification.Schedule(NotificationId, "タイマー", "時間になりました", countdown.GetRemaining(now));
        ApplyCountdown();
    }

    private void PauseCountdown()
    {
        countdown.Pause(timeProvider.GetUtcNow());
        notification.Cancel(NotificationId);
        ApplyCountdown();
    }

    private void ResetCountdown()
    {
        countdown.Reset();
        notification.Cancel(NotificationId);
        ApplyCountdown();
    }

    private void SetDuration(int minutes)
    {
        countdown.SetDuration(TimeSpan.FromMinutes(minutes));
        notification.Cancel(NotificationId);
        ApplyCountdown();
    }

    private async Task InputDurationAsync()
    {
        var text = await popupNavigator.InputNumberAsync("分", DurationMinutes.ToString(CultureInfo.InvariantCulture), 3);
        if (Int32.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var minutes) && (minutes > 0))
        {
            SetDuration(minutes);
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void ApplyStopwatch()
    {
        StopwatchState = stopwatch.State;
        Refresh();
        UpdateTicker();
        Save();
    }

    private void ApplyCountdown()
    {
        CountdownState = countdown.State;
        Duration = countdown.Duration;
        DurationMinutes = (int)countdown.Duration.TotalMinutes;
        Refresh();
        UpdateTicker();
        Save();
    }

    // 刻みを数えず、今の時刻で計算し直す。表示中に終わったら振動で知らせ、予約の通知は取り消す
    private void Refresh()
    {
        var now = timeProvider.GetUtcNow();

        Elapsed = stopwatch.GetElapsed(now);
        CurrentLap = stopwatch.GetCurrentLap(now);
        LapNo = stopwatch.LapCount + 1;
        StopwatchProgress = stopwatch.GetMinuteProgress(now);

        if (countdown.CheckFinished(now))
        {
            notification.Cancel(NotificationId);
            if (visible && session.IsForeground)
            {
                vibration.Vibrate(FinishVibration);
            }

            CountdownState = countdown.State;
            UpdateTicker();
            Save();
        }

        Remaining = countdown.GetRemaining(now);
        CountdownProgress = countdown.GetProgress(now);
        IsWarning = countdown.IsWarning(now);
        EndTime = countdown.EndAt is { } end ? TimeZoneInfo.ConvertTime(end, timeProvider.LocalTimeZone).DateTime : null;
    }

    // 画面を表示していて前面で、計測中の間だけ表示を更新し、画面を消灯させない
    private void UpdateTicker()
    {
        var running = (stopwatch.State == TimerStopwatchState.Running) || (countdown.State == TimerCountdownState.Running);
        if (visible && session.IsForeground && running)
        {
            ticker.Start();
        }
        else
        {
            ticker.Stop();
        }

        screen.KeepScreenOn(visible && running);
    }

    // 状態は JSON で持つ (初めては 3 分)
    private void Load()
    {
        if (store.Load(AppJsonContext.Default.TimerSnapshot) is { } snapshot)
        {
            stopwatch.Import(snapshot.Stopwatch);
            countdown.Import(snapshot.Countdown);
        }

        if (countdown.Duration == TimeSpan.Zero)
        {
            countdown.SetDuration(TimeSpan.FromMinutes(3));
        }

        StopwatchState = stopwatch.State;
        CountdownState = countdown.State;
        Duration = countdown.Duration;
        DurationMinutes = (int)countdown.Duration.TotalMinutes;
    }

    private void Save() =>
        store.Save(
            new TimerSnapshot { Stopwatch = stopwatch.Export(), Countdown = countdown.Export() },
            AppJsonContext.Default.TimerSnapshot);
}
