namespace Template.MobileApp.State;

#pragma warning disable CA1008
[Flags]
public enum NetworkProfile
{
    Unknown = 0,
    Bluetooth = 0x01,
    Cellular = 0x02,
    Ethernet = 0x04,
    WiFi = 0x08
}
#pragma warning restore CA1008

public enum NetworkState
{
    Connected,
    ConnectedHighSpeed,
    Disconnected
}

public static class DeviceStateExtensions
{
    public static bool IsConnected(this NetworkAccess access) =>
        access != NetworkAccess.None && access != NetworkAccess.Unknown;

    public static bool IsHighSpeed(this NetworkProfile profile) =>
        profile.HasFlag(NetworkProfile.Ethernet) || profile.HasFlag(NetworkProfile.WiFi);

    public static bool IsConnected(this NetworkState state) =>
        state is NetworkState.ConnectedHighSpeed or NetworkState.Connected;
}

public sealed partial class DeviceState : ObservableObject, IDisposable
{
    private readonly ILogger<DeviceState> log;

    private readonly List<IDisposable> disposables = [];

    // Battery

    [ObservableProperty]
    public partial double BatteryChargeLevel { get; private set; }

    [ObservableProperty]
    public partial BatteryState BatteryState { get; private set; }

    [ObservableProperty]
    public partial BatteryPowerSource BatteryPowerSource { get; private set; }

    // Connectivity

    [ObservableProperty]
    public partial NetworkProfile NetworkProfile { get; private set; }

    [ObservableProperty]
    public partial NetworkAccess NetworkAccess { get; private set; }

    [ObservableProperty]
    public partial NetworkState NetworkState { get; private set; }

    public DeviceState(
        ILogger<DeviceState> log,
        IBattery battery,
        IConnectivity connectivity)
    {
        this.log = log;

        // Battery
        UpdateBattery(battery.ChargeLevel, battery.State, battery.PowerSource);
        disposables.Add(battery.BatteryInfoChangedAsObservable().ObserveOnCurrentContext().Subscribe(
            x => UpdateBattery(x.ChargeLevel, x.State, x.PowerSource)));
        // Connectivity
        UpdateConnectivity(connectivity.ConnectionProfiles, connectivity.NetworkAccess);
        disposables.Add(connectivity.ConnectivityChangedAsObservable().ObserveOnCurrentContext().Subscribe(
            x => UpdateConnectivity(x.ConnectionProfiles, x.NetworkAccess)));
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }

        disposables.Clear();
    }

    // ------------------------------------------------------------
    // Battery
    // ------------------------------------------------------------

    private void UpdateBattery(double chargeLevel, BatteryState state, BatteryPowerSource powerSource)
    {
        log.DebugBatteryState(chargeLevel, state, powerSource);

        BatteryChargeLevel = chargeLevel;
        BatteryState = state;
        BatteryPowerSource = powerSource;
    }

    // ------------------------------------------------------------
    // Connectivity
    // ------------------------------------------------------------

    private void UpdateConnectivity(IEnumerable<ConnectionProfile> profiles, NetworkAccess access)
    {
        var profile = NetworkProfile.Unknown;
        foreach (var value in profiles)
        {
            switch (value)
            {
                case ConnectionProfile.Bluetooth:
                    profile |= NetworkProfile.Bluetooth;
                    break;
                case ConnectionProfile.Cellular:
                    profile |= NetworkProfile.Cellular;
                    break;
                case ConnectionProfile.Ethernet:
                    profile |= NetworkProfile.Ethernet;
                    break;
                case ConnectionProfile.WiFi:
                    profile |= NetworkProfile.WiFi;
                    break;
            }
        }

        log.DebugConnectivityState(profile, access);

        NetworkProfile = profile;
        NetworkAccess = access;
        NetworkState = access.IsConnected()
            ? (profile.IsHighSpeed() ? NetworkState.ConnectedHighSpeed : NetworkState.Connected)
            : NetworkState.Disconnected;
    }
}
