namespace Template.MobileApp.Modules.Device;

public sealed partial class DeviceWiFiViewModel : AppViewModelBase
{
    // Field

    private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(30);

    private static readonly TimeSpan RemoveAfter = TimeSpan.FromSeconds(60);

    private static readonly TimeSpan ExpireInterval = TimeSpan.FromSeconds(5);

    private readonly IWiFiManager wifiManager;

    private IReadOnlyList<WiFiAccessPoint>? lastResults;

    private SerialDisposable ScanTimer { get; } = new();

    private SerialDisposable ExpireTimer { get; } = new();

    // Connection

    [ObservableProperty]
    public partial bool IsSupported { get; set; }

    [ObservableProperty]
    public partial bool IsRadioOn { get; set; }

    [ObservableProperty]
    public partial bool IsConnected { get; set; }

    [ObservableProperty]
    public partial bool HasPermission { get; set; }

    [ObservableProperty]
    public partial bool IsDetailOpen { get; set; }

    [ObservableProperty]
    public partial string StateText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial WiFiConnection? Connection { get; set; }

    // List

    public ObservableCollection<WiFiAccessPointItem> AccessPoints { get; } = [];

    [ObservableProperty]
    public partial int Band24Count { get; set; }

    [ObservableProperty]
    public partial int Band5Count { get; set; }

    [ObservableProperty]
    public partial int Band6Count { get; set; }

    [ObservableProperty]
    public partial bool HasBand6 { get; set; }

    [ObservableProperty]
    public partial string ScanMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool Scanning { get; set; }

    // Command

    public IObserveCommand ToggleDetailCommand { get; }

    public IObserveCommand ToggleExpandCommand { get; }

    public IObserveCommand OpenSettingsCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DeviceWiFiViewModel(IWiFiManager wifiManager)
    {
        this.wifiManager = wifiManager;

        ToggleDetailCommand = MakeDelegateCommand(() => IsDetailOpen = !IsDetailOpen);
        ToggleExpandCommand = MakeDelegateCommand<WiFiAccessPointItem>(x => x.IsExpanded = !x.IsExpanded);
        OpenSettingsCommand = MakeDelegateCommand(wifiManager.OpenSettings);

        Disposables.Add(wifiManager.StateChangedAsObservable().ObserveOnCurrentContext().Subscribe(_ => Update()));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        IsSupported = wifiManager.IsSupported;
        HasPermission = await Permissions.RequestLocationAsync() && await Permissions.RequestNearbyWifiDevicesAsync();
        if (IsSupported)
        {
            wifiManager.Enabled = true;
            Scan();
            StartTimers();
        }

        Update();
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        StopTimers();
        wifiManager.Enabled = false;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4()
    {
        wifiManager.OpenSettings();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void StartTimers()
    {
        StopTimers();
        ExpireTimer.Disposable = Observable.Interval(ExpireInterval).ObserveOnCurrentContext().Subscribe(_ => Expire(DateTime.Now));
        ScheduleScan();
    }

    private void StopTimers()
    {
        ScanTimer.Disposable = null;
        ExpireTimer.Disposable = null;
    }

    private void ScheduleScan()
    {
        if (ExpireTimer.Disposable is null)
        {
            return;
        }

        ScanTimer.Disposable = Observable.Timer(ScanInterval).ObserveOnCurrentContext().Subscribe(_ => Scan());
    }

    private void Scan()
    {
        if (wifiManager.StartScan())
        {
            ScanMessage = "Scanning…";
            Scanning = true;
        }

        ScheduleScan();
    }

    private void Update()
    {
        IsRadioOn = wifiManager.IsRadioOn;
        Connection = wifiManager.Connection;
        IsConnected = Connection is not null;
        StateText = !IsSupported ? "Not supported" : !IsRadioOn ? "Wi-Fi off" : IsConnected ? "Connected" : "Not connected";

        UpdateAccessPoints(Connection?.Bssid);
    }

    private void UpdateAccessPoints(string? connectedBssid)
    {
        var now = DateTime.Now;
        var results = wifiManager.AccessPoints;
        if (!ReferenceEquals(results, lastResults))
        {
            lastResults = results;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var result in results)
            {
                seen.Add(result.Bssid);
                var item = Find(result.Bssid);
                if (item is null)
                {
                    AccessPoints.Add(new WiFiAccessPointItem(result));
                }
                else
                {
                    item.Update(result);
                }
            }

            foreach (var item in AccessPoints)
            {
                if (!seen.Contains(item.AccessPoint.Bssid))
                {
                    item.Miss(now);
                }
            }

            ScanMessage = results.Count > 0 ? $"Updated {now:HH:mm:ss}" : HasPermission ? "No access points" : "Permission required for scan";
            Scanning = false;
            ScheduleScan();
        }

        foreach (var item in AccessPoints)
        {
            item.IsConnected = String.Equals(item.AccessPoint.Bssid, connectedBssid, StringComparison.OrdinalIgnoreCase);
        }

        Expire(now);
    }

    private WiFiAccessPointItem? Find(string bssid) =>
        AccessPoints.FirstOrDefault(x => String.Equals(x.AccessPoint.Bssid, bssid, StringComparison.OrdinalIgnoreCase));

    private void Expire(DateTime now)
    {
        for (var i = AccessPoints.Count - 1; i >= 0; i--)
        {
            if (AccessPoints[i].MissedSince is { } missed && (now - missed > RemoveAfter))
            {
                AccessPoints.RemoveAt(i);
            }
        }

        var ordered = AccessPoints
            .OrderByDescending(static x => x.IsConnected)
            .ThenBy(static x => x.IsStale)
            .ThenByDescending(static x => x.AccessPoint.Rssi)
            .ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            var index = AccessPoints.IndexOf(ordered[i]);
            if (index != i)
            {
                AccessPoints.Move(index, i);
            }
        }

        Band24Count = AccessPoints.Count(static x => x.AccessPoint.Frequency < 4900);
        Band5Count = AccessPoints.Count(static x => x.AccessPoint.Frequency is >= 4900 and < 5925);
        Band6Count = AccessPoints.Count(static x => x.AccessPoint.Frequency >= 5925);
        HasBand6 = Band6Count > 0;
    }
}

public sealed partial class WiFiAccessPointItem : ObservableObject
{
    public DateTime? MissedSince { get; private set; }

    [ObservableProperty]
    public partial WiFiAccessPoint AccessPoint { get; set; }

    [ObservableProperty]
    public partial bool IsConnected { get; set; }

    [ObservableProperty]
    public partial bool IsStale { get; set; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    public WiFiAccessPointItem(WiFiAccessPoint accessPoint)
    {
        AccessPoint = accessPoint;
    }

    public void Update(WiFiAccessPoint value)
    {
        AccessPoint = value;
        MissedSince = null;
        IsStale = false;
    }

    public void Miss(DateTime now)
    {
        if (MissedSince is null)
        {
            MissedSince = now;
            IsStale = true;
        }
    }
}
