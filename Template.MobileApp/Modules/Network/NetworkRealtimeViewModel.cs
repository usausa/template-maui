namespace Template.MobileApp.Modules.Network;

using Mofucat.ReactiveHub;

using Template.MobileApp.Components;
using Template.MobileApp.Services;

public enum RealtimeState
{
    Stopped,
    Connecting,
    Connected,
    Reconnecting,
    Error
}

public sealed partial class NetworkRealtimeViewModel : AppViewModelBase
{
    private const int NotificationId = 100;

    private const int HistorySize = 61;

    // Field

    private static readonly TimeSpan ReportInterval = TimeSpan.FromSeconds(10);

    private readonly ILogger<NetworkRealtimeViewModel> log;

    private readonly IDeviceInfo deviceInfo;

    private readonly IDispatcherTimer reportTimer;

    private readonly MonitorConnection connection;

    private readonly ApiContext apiContext;

    private readonly INotificationService notification;

    private readonly Settings settings;

    private readonly DeviceState deviceState;

    // Subscription

    private SerialDisposable Receiving { get; } = new();
    private SerialDisposable Connecting { get; } = new();

    // Connection

    [ObservableProperty]
    public partial RealtimeState State { get; set; }

    [ObservableProperty]
    public partial string ConnectionId { get; set; } = "-";

    [ObservableProperty]
    public partial string LastError { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ServerTime { get; set; } = "-";

    [ObservableProperty]
    public partial int Connections { get; set; }

    [ObservableProperty]
    public partial int ReportCount { get; set; }

    // Chart

    public StatDataSet CpuLoadSet { get; } = new(HistorySize);
    public StatDataSet MemoryLoadSet { get; } = new(HistorySize);
    public StatDataSet ConnectionSet { get; } = new(HistorySize);

    // Notifications

    public ObservableCollection<string> Notifications { get; } = [];

    // Command

    public IObserveCommand ReconnectCommand { get; }

    public IObserveCommand ReportCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public NetworkRealtimeViewModel(
        ILogger<NetworkRealtimeViewModel> log,
        IDeviceInfo deviceInfo,
        MonitorConnection connection,
        ApiContext apiContext,
        INotificationService notification,
        Settings settings,
        DeviceState deviceState)
    {
        this.log = log;
        this.deviceInfo = deviceInfo;
        this.connection = connection;
        this.apiContext = apiContext;
        this.notification = notification;
        this.settings = settings;
        this.deviceState = deviceState;

        ReconnectCommand = MakeDelegateCommand(Connect);
        ReportCommand = MakeAsyncCommand(ReportAsync);

        reportTimer = Application.Current?.Dispatcher.CreateTimer()!;
        reportTimer.Interval = ReportInterval;
        Disposables.Add(reportTimer.TickAsObservable().Subscribe(x => _ = ReportAsync()));
        Disposables.Add(new DelegateDisposable(reportTimer.Stop));
        Disposables.Add(Receiving);
        Disposables.Add(Connecting);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        Receiving.Disposable = new CompositeDisposable(
            connection.ServerStatus.ObserveOnCurrentContext().Subscribe(OnServerStatus, OnError),
            connection.Notifications.ObserveOnCurrentContext().Subscribe(OnNotification, OnError));
        Connect();
        reportTimer.Start();

        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        if (Receiving.Disposable is not null)
        {
            reportTimer.Stop();
            Disconnect();
            Receiving.Disposable = null;
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Connect();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Connection
    //--------------------------------------------------------------------------------

    private void Connect()
    {
        Disconnect();
        Connecting.Disposable = connection.Connect(apiContext.BaseAddress!)
            .ObserveOnCurrentContext()
            .Subscribe(OnStatus, OnError);
    }

    private void Disconnect()
    {
        Connecting.Disposable = null;
        State = RealtimeState.Stopped;
        ConnectionId = "-";
        LastError = string.Empty;
    }

    private void OnStatus(HubStatus status)
    {
        State = status.Kind switch
        {
            HubStatusKind.Connecting => RealtimeState.Connecting,
            HubStatusKind.Connected => RealtimeState.Connected,
            HubStatusKind.Reconnecting => RealtimeState.Reconnecting,
            _ => RealtimeState.Stopped
        };
        ConnectionId = status.ConnectionId ?? "-";
        LastError = status.Error?.Message ?? string.Empty;

        if (status.Kind == HubStatusKind.Connected)
        {
            _ = ReportAsync();
        }
    }

    private void OnError(Exception ex)
    {
        log.WarnMonitorError(ex);
        State = RealtimeState.Error;
        LastError = ex.Message;
    }

    private void OnServerStatus(ServerStatusMessage status)
    {
        ServerTime = status.Time.LocalDateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        Connections = status.Connections;
        CpuLoadSet.Add((int)Math.Round(status.CpuPercent));
        MemoryLoadSet.Add((int)(status.WorkingSet / (1024 * 1024)));
        ConnectionSet.Add(status.Connections);
    }

    private void OnNotification(NotificationMessage item)
    {
        Notifications.Insert(0, $"{item.SentAt.LocalDateTime:HH:mm:ss} {item.Title}: {item.Body}");
        while (Notifications.Count > 20)
        {
            Notifications.RemoveAt(Notifications.Count - 1);
        }

        notification.Show(NotificationId, item.Title, item.Body);
    }

    //--------------------------------------------------------------------------------
    // Report
    //--------------------------------------------------------------------------------

    private async Task ReportAsync()
    {
        if (!connection.IsConnected)
        {
            return;
        }

        await connection.ReportDeviceStatusAsync(new DeviceStatusMessage
        {
            TerminalId = settings.TerminalId,
            Model = $"{deviceInfo.Manufacturer} {deviceInfo.Model}",
            Platform = $"{deviceInfo.Platform} {deviceInfo.VersionString}",
            Battery = deviceState.BatteryChargeLevel,
            BatteryState = deviceState.BatteryState.ToString(),
            Network = $"{deviceState.NetworkProfile} {deviceState.NetworkAccess}"
        });
        ReportCount++;
    }
}
