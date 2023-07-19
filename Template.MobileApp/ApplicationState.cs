namespace Template.MobileApp;

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

public static class ApplicationStateExtensions
{
    public static bool IsConnected(this NetworkAccess access) =>
        access != NetworkAccess.None && access != NetworkAccess.Unknown;

    public static bool IsHighSpeed(this NetworkProfile profile) =>
        profile.HasFlag(NetworkProfile.Ethernet) || profile.HasFlag(NetworkProfile.WiFi);

    public static bool IsConnected(this NetworkState state) =>
        state == NetworkState.ConnectedHighSpeed || state == NetworkState.Connected;
}

public sealed class ApplicationState : BusyState, IDisposable
{
    private readonly ILogger<ApplicationState> log;

    private readonly List<IDisposable> disposables = new();

    // Battery

    private double batteryChargeLevel;

    public double BatteryChargeLevel
    {
        get => batteryChargeLevel;
        private set => SetProperty(ref batteryChargeLevel, value);
    }

    private BatteryState batteryState;

    public BatteryState BatteryState
    {
        get => batteryState;
        private set => SetProperty(ref batteryState, value);
    }

    private BatteryPowerSource batteryPowerSource;

    public BatteryPowerSource BatteryPowerSource
    {
        get => batteryPowerSource;
        private set => SetProperty(ref batteryPowerSource, value);
    }

    // Connectivity

    private NetworkProfile networkProfile;

    public NetworkProfile NetworkProfile
    {
        get => networkProfile;
        private set => SetProperty(ref networkProfile, value);
    }
    private NetworkAccess networkAccess;

    public NetworkAccess NetworkAccess
    {
        get => networkAccess;
        private set => SetProperty(ref networkAccess, value);
    }

    private NetworkState networkState;

    public NetworkState NetworkState
    {
        get => networkState;
        private set => SetProperty(ref networkState, value);
    }

    public ApplicationState(
        ILogger<ApplicationState> log,
        IBattery battery,
        IConnectivity connectivity)
    {
        this.log = log;

        // Battery
        UpdateBattery(battery.ChargeLevel, battery.State, battery.PowerSource);
        disposables.Add(battery.ObserveBatteryInfoChangedOnCurrentContext().Subscribe(
            x => UpdateBattery(x.ChargeLevel, x.State, x.PowerSource)));
        // Connectivity
        UpdateConnectivity(connectivity.ConnectionProfiles, connectivity.NetworkAccess);
        disposables.Add(connectivity.ObserveConnectivityChangedOnCurrentContext().Subscribe(
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
