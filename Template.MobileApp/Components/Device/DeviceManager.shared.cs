namespace Template.MobileApp.Components.Device;

public sealed partial class DeviceManager : IDeviceManager, IDisposable
{
    private readonly BehaviorSubject<NetworkState> networkState;

    public IObservable<NetworkState> NetworkState => networkState;

    public DeviceManager()
    {
        networkState = new BehaviorSubject<NetworkState>(GetNetworkState(Connectivity.NetworkAccess, Connectivity.ConnectionProfiles));
        Connectivity.ConnectivityChanged += (_, args) =>
        {
            networkState.OnNext(GetNetworkState(args.NetworkAccess, args.ConnectionProfiles));
        };
    }

    public void Dispose()
    {
        networkState.Dispose();
    }

    private static NetworkState GetNetworkState(NetworkAccess access, IEnumerable<ConnectionProfile> profiles)
    {
        if (access != NetworkAccess.None && access != NetworkAccess.Unknown)
        {
            return profiles.Any(x => x == ConnectionProfile.Ethernet || x == ConnectionProfile.WiFi)
                ? Template.MobileApp.Components.Device.NetworkState.ConnectedHighSpeed
                : Template.MobileApp.Components.Device.NetworkState.Connected;
        }

        return Template.MobileApp.Components.Device.NetworkState.Disconnected;
    }

    public NetworkState GetNetworkState() => GetNetworkState(Connectivity.NetworkAccess, Connectivity.ConnectionProfiles);
}
