namespace Template.MobileApp.Modules.Device;

public sealed partial class DeviceWiFiViewModel : AppViewModelBase
{
    // 最後のスキャン要求または結果からこの時間が経ったら自前でスキャンする (他者のスキャン結果が届いていれば撃たない)。
    // 前面アプリのスキャンは 2 分に 4 回まで
    private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(30);

    // 更新時に検出されなかったアクセスポイントを消すまでの猶予
    private static readonly TimeSpan RemoveAfter = TimeSpan.FromSeconds(60);

    private static readonly TimeSpan ExpireInterval = TimeSpan.FromSeconds(5);

    private readonly IWiFiManager wifiManager;

    // 直近に処理したスキャン結果 (同じ結果の再通知では未検出の判定をしない)
    private IReadOnlyList<WiFiAccessPoint>? lastResults;

    // 自前スキャンの予約と未検出の期限切れ (表示中だけ)。破棄は Disposables に任せる
    private SerialDisposable ScanTimer { get; } = new();

    private SerialDisposable ExpireTimer { get; } = new();

    [ObservableProperty]
    public partial bool IsSupported { get; set; }

    [ObservableProperty]
    public partial bool IsRadioOn { get; set; }

    [ObservableProperty]
    public partial bool IsConnected { get; set; }

    // 位置情報 (+ Android 13 以降は NEARBY_WIFI_DEVICES) の権限が無いと SSID とスキャン結果は取得できない
    [ObservableProperty]
    public partial bool HasPermission { get; set; }

    [ObservableProperty]
    public partial bool IsDetailOpen { get; set; }

    [ObservableProperty]
    public partial string StateText { get; set; } = string.Empty;

    // 接続情報 (未接続は null)。項目は XAML から直接バインドする
    [ObservableProperty]
    public partial WiFiConnection? Connection { get; set; }

    // 検出したアクセスポイント (接続中 → 検出中を電波の強い順 → 未検出)。更新は行を差し替えず、その場で値を更新して並べ替える
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

    // 表示中は未検出のアクセスポイントを猶予を過ぎたら消し、結果が途絶えたら自前でスキャンする
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

    // 自動スキャンを ScanInterval 後に予約し直す (スキャン要求と結果の受信のたびに延期される)
    private void ScheduleScan()
    {
        if (ExpireTimer.Disposable is null)
        {
            return;
        }

        ScanTimer.Disposable = Observable.Timer(ScanInterval).ObserveOnCurrentContext().Subscribe(_ => Scan());
    }

    // 結果は StateChanged 経由で反映。前面アプリのスキャンは 2 分に 4 回までで、超えると要求が拒否される (次回に回す)
    private void Scan()
    {
        if (wifiManager.StartScan())
        {
            ScanMessage = "スキャン中…";
        }

        ScheduleScan();
    }

    private void Update()
    {
        IsRadioOn = wifiManager.IsRadioOn;
        Connection = wifiManager.Connection;
        IsConnected = Connection is not null;
        StateText = !IsSupported ? "非対応" : !IsRadioOn ? "Wi-Fi オフ" : IsConnected ? "接続中" : "未接続";

        UpdateAccessPoints(Connection?.Bssid);
    }

    private void UpdateAccessPoints(string? connectedBssid)
    {
        var now = DateTime.Now;
        var results = wifiManager.AccessPoints;
        if (!ReferenceEquals(results, lastResults))
        {
            // 新しいスキャン結果: 既存の行はその場で更新し、含まれない行は未検出にする
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

            ScanMessage = results.Count > 0 ? $"更新 {now:HH:mm:ss}" : HasPermission ? "アクセスポイントは検出されていません" : "権限が無いためスキャン結果を取得できません";
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

    // 猶予を過ぎた未検出の行を消し、並び順と内訳を更新する
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

// 検出したアクセスポイントの行。値は AccessPoint の差し替えで更新し、項目は XAML から直接バインドする (文言はコンバーター)
public sealed partial class WiFiAccessPointItem : ObservableObject
{
    // 更新で検出されなくなった時刻 (検出中は null)
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
