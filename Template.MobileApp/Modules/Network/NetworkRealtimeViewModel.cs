namespace Template.MobileApp.Modules.Network;

using Mofucat.ReactiveHub;

using Template.MobileApp.Components;
using Template.MobileApp.Services;

public sealed partial class NetworkRealtimeViewModel : AppViewModelBase
{
    private const int NotificationId = 100;

    private const int HistorySize = 61;

    private static readonly TimeSpan ReportInterval = TimeSpan.FromSeconds(10);

    private readonly ILogger<NetworkRealtimeViewModel> log;

    private readonly IDeviceInfo deviceInfo;

    private readonly IDialog dialog;

    private readonly IDispatcherTimer reportTimer;

    private readonly MonitorConnection connection;

    private readonly ApiContext apiContext;

    private readonly INotificationService notification;

    private readonly Settings settings;

    private readonly Session session;

    private readonly DeviceState deviceState;

    private SerialDisposable Receiving { get; } = new();
    private SerialDisposable Connecting { get; } = new();

    [ObservableProperty]
    public partial bool Configured { get; set; }

    [ObservableProperty]
    public partial string StateText { get; set; } = "停止";

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

    public StatDataSet CpuLoadSet { get; } = new(HistorySize);
    public StatDataSet MemoryLoadSet { get; } = new(HistorySize);
    public StatDataSet ConnectionSet { get; } = new(HistorySize);

    public ObservableCollection<string> Notifications { get; } = [];

    public IObserveCommand ReconnectCommand { get; }

    public IObserveCommand ReportCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public NetworkRealtimeViewModel(
        ILogger<NetworkRealtimeViewModel> log,
        IDeviceInfo deviceInfo,
        IDialog dialog,
        MonitorConnection connection,
        ApiContext apiContext,
        INotificationService notification,
        Settings settings,
        Session session,
        DeviceState deviceState)
    {
        this.log = log;
        this.deviceInfo = deviceInfo;
        this.dialog = dialog;
        this.connection = connection;
        this.apiContext = apiContext;
        this.notification = notification;
        this.settings = settings;
        this.session = session;
        this.deviceState = deviceState;

        ReconnectCommand = MakeDelegateCommand(Connect, () => Configured);
        ReportCommand = MakeAsyncCommand(ReportAsync, () => Configured);

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

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        Configured = settings.IsApiConfigured();
        if (!Configured)
        {
            StateText = "未設定";
        }

        return Task.CompletedTask;
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (Configured)
        {
            // 受信はバックグラウンドスレッドから来る。Rx の未処理 OnError はアプリを落とすため必ず処理する
            Receiving.Disposable = new CompositeDisposable(
                connection.ServerStatus.ObserveOnCurrentContext().Subscribe(OnServerStatus, OnError),
                connection.Notifications.ObserveOnCurrentContext().Subscribe(OnNotification, OnError));
            Connect();
            reportTimer.Start();
        }

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
        if (Configured)
        {
            Connect();
        }

        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Connection
    //--------------------------------------------------------------------------------

    // 接続を始める (動作中なら繋ぎ直す)。購読が接続の維持そのもので、破棄が切断
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
        StateText = "停止";
        ConnectionId = "-";
        LastError = string.Empty;
    }

    private void OnStatus(HubStatus status)
    {
        StateText = status.Kind switch
        {
            HubStatusKind.Connecting => "接続中...",
            HubStatusKind.Connected => "接続済み",
            HubStatusKind.Reconnecting => "再接続中...",
            _ => "停止"
        };
        ConnectionId = status.ConnectionId ?? "-";
        LastError = status.Error?.Message ?? string.Empty;

        // 接続したら状態をすぐに報告する
        if (status.Kind == HubStatusKind.Connected)
        {
            _ = ReportAsync();
        }
    }

    private void OnError(Exception ex)
    {
        log.WarnMonitorError(ex);
        StateText = "エラー";
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

    // 前面ならトースト、バックグラウンドならローカル通知
    private void OnNotification(NotificationMessage item)
    {
        Notifications.Insert(0, $"{item.SentAt.LocalDateTime:HH:mm:ss} {item.Title}: {item.Body}");
        while (Notifications.Count > 20)
        {
            Notifications.RemoveAt(Notifications.Count - 1);
        }

        if (session.IsForeground)
        {
            _ = dialog.Toast($"{item.Title}: {item.Body}").AsTask();
        }
        else
        {
            notification.Show(NotificationId, item.Title, item.Body);
        }
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
            DeviceId = settings.UniqueId,
            Model = $"{deviceInfo.Manufacturer} {deviceInfo.Model}",
            Platform = $"{deviceInfo.Platform} {deviceInfo.VersionString}",
            Battery = deviceState.BatteryChargeLevel,
            BatteryState = deviceState.BatteryState.ToString(),
            Network = $"{deviceState.NetworkProfile} {deviceState.NetworkAccess}"
        });
        ReportCount++;
    }
}
