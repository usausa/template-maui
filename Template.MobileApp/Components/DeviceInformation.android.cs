namespace Template.MobileApp.Components;

using System.Buffers.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;

using AndroidX.Core.Content;

using Microsoft.Win32.SafeHandles;

using AndroidBatteryManager = Android.OS.BatteryManager;
using AndroidBatteryPlugged = Android.OS.BatteryPlugged;
using AndroidBatteryStatus = Android.OS.BatteryStatus;
using AndroidNetwork = Android.Net.Network;

public sealed partial class DeviceInformation
{
    private const int StatBufferSize = 512;

    private const int ThreadCountField = 20;
    private const int ResidentPagesField = 24;

    private static readonly long PageSize = Environment.SystemPageSize;

    private readonly BatteryReceiver batteryReceiver;

    private readonly NetworkCallback networkCallback;

    private readonly Dictionary<long, NetworkEntry> networks = [];

    private readonly SafeFileHandle statHandle = File.OpenHandle("/proc/self/stat");

    private ConnectivityManager? connectivityManager;

    private bool networkRegistered;

    private Transport currentTransports;

    public DeviceInformation()
    {
        batteryReceiver = new BatteryReceiver(this);
        networkCallback = new NetworkCallback(this);
    }

    public void Dispose()
    {
        Stop();

        networkCallback.Dispose();
        batteryReceiver.Dispose();
        statHandle.Dispose();
    }

    // ------------------------------------------------------------
    // Identity
    // ------------------------------------------------------------

    private static partial string ResolveDeviceId() =>
        Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId) ?? string.Empty;

    // ------------------------------------------------------------
    // Battery
    // ------------------------------------------------------------

    private partial void StartBattery()
    {
        using var filter = new IntentFilter(Intent.ActionBatteryChanged);
        using var intent = ContextCompat.RegisterReceiver(Application.Context, batteryReceiver, filter, ContextCompat.ReceiverNotExported);
        if (intent is not null)
        {
            OnBatteryChanged(intent);
        }
    }

    private partial void StopBattery() => Application.Context.UnregisterReceiver(batteryReceiver);

    private void OnBatteryChanged(Intent intent)
    {
        var level = intent.GetIntExtra(AndroidBatteryManager.ExtraLevel, -1);
        var scale = intent.GetIntExtra(AndroidBatteryManager.ExtraScale, -1);
        var status = (AndroidBatteryStatus)intent.GetIntExtra(AndroidBatteryManager.ExtraStatus, (int)AndroidBatteryStatus.Unknown);
        var plugged = (AndroidBatteryPlugged)intent.GetIntExtra(AndroidBatteryManager.ExtraPlugged, 0);

        var state = status switch
        {
            AndroidBatteryStatus.Charging => BatteryState.Charging,
            AndroidBatteryStatus.Discharging => BatteryState.Discharging,
            AndroidBatteryStatus.Full => BatteryState.Full,
            AndroidBatteryStatus.NotCharging => BatteryState.NotCharging,
            _ => BatteryState.Unknown
        };
        var powerSource = plugged switch
        {
            AndroidBatteryPlugged.Ac => BatteryPowerSource.AC,
            AndroidBatteryPlugged.Usb => BatteryPowerSource.Usb,
            AndroidBatteryPlugged.Wireless => BatteryPowerSource.Wireless,
            _ => BatteryPowerSource.Battery
        };

        UpdateBattery(new BatteryStatus((level >= 0) && (scale > 0) ? (double)level / scale : -1, state, powerSource));
    }

    private sealed class BatteryReceiver : BroadcastReceiver
    {
        private readonly DeviceInformation owner;

        public BatteryReceiver(DeviceInformation owner)
        {
            this.owner = owner;
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent is not null)
            {
                owner.OnBatteryChanged(intent);
            }
        }
    }

    // ------------------------------------------------------------
    // Network
    // ------------------------------------------------------------

    [Flags]
    private enum Transport
    {
        None = 0,
        Bluetooth = 0x01,
        Cellular = 0x02,
        Ethernet = 0x04,
        WiFi = 0x08
    }

    private readonly record struct NetworkEntry(Transport Transport, bool Validated, WiFiStatus? WiFi);

    private partial void StartNetwork()
    {
        connectivityManager ??= (ConnectivityManager?)Application.Context.GetSystemService(Context.ConnectivityService);
        using var builder = new NetworkRequest.Builder();
        using var request = builder.Build();
        if ((connectivityManager is null) || (request is null))
        {
            return;
        }

        networks.Clear();

        connectivityManager.RegisterNetworkCallback(request, networkCallback);
        networkRegistered = true;
    }

    private partial void StopNetwork()
    {
        if (networkRegistered)
        {
            connectivityManager?.UnregisterNetworkCallback(networkCallback);
            networkRegistered = false;
        }
    }

    private void OnCapabilitiesChanged(AndroidNetwork network, NetworkCapabilities capabilities)
    {
        var transport = Transport.None;
        if (capabilities.HasTransport(TransportType.Bluetooth))
        {
            transport |= Transport.Bluetooth;
        }
        if (capabilities.HasTransport(TransportType.Cellular))
        {
            transport |= Transport.Cellular;
        }
        if (capabilities.HasTransport(TransportType.Ethernet))
        {
            transport |= Transport.Ethernet;
        }
        if (capabilities.HasTransport(TransportType.Wifi))
        {
            transport |= Transport.WiFi;
        }

        var wifi = OperatingSystem.IsAndroidVersionAtLeast(29) && (capabilities.TransportInfo is WifiInfo info)
            ? new WiFiStatus(info.Rssi, info.LinkSpeed)
            : null;

        networks[network.NetworkHandle] = new NetworkEntry(transport, capabilities.HasCapability(NetCapability.Validated), wifi);
        Refresh();
    }

    private void OnLost(AndroidNetwork network)
    {
        networks.Remove(network.NetworkHandle);
        Refresh();
    }

    private void Refresh()
    {
        var access = NetworkAccess.None;
        var transports = Transport.None;
        WiFiStatus? wifi = null;
        foreach (var entry in networks.Values)
        {
            if (entry.Validated)
            {
                access = NetworkAccess.Internet;
            }
            else if (access != NetworkAccess.Internet)
            {
                access = NetworkAccess.ConstrainedInternet;
            }

            transports |= entry.Transport;
            wifi ??= entry.WiFi;
        }

        if ((Network is null) || (access != Network.Access) || (transports != currentTransports))
        {
            currentTransports = transports;
            UpdateNetwork(new NetworkStatus(access, ToProfiles(transports)));
        }

        if (wifi != WiFi)
        {
            UpdateWiFi(wifi);
        }
    }

    private static ConnectionProfile[] ToProfiles(Transport transports)
    {
        var profiles = new List<ConnectionProfile>(4);
        if ((transports & Transport.Bluetooth) != 0)
        {
            profiles.Add(ConnectionProfile.Bluetooth);
        }
        if ((transports & Transport.Cellular) != 0)
        {
            profiles.Add(ConnectionProfile.Cellular);
        }
        if ((transports & Transport.Ethernet) != 0)
        {
            profiles.Add(ConnectionProfile.Ethernet);
        }
        if ((transports & Transport.WiFi) != 0)
        {
            profiles.Add(ConnectionProfile.WiFi);
        }

        return [.. profiles];
    }

    private sealed class NetworkCallback : ConnectivityManager.NetworkCallback
    {
        private readonly DeviceInformation owner;

        public NetworkCallback(DeviceInformation owner)
        {
            this.owner = owner;
        }

        public override void OnCapabilitiesChanged(AndroidNetwork network, NetworkCapabilities networkCapabilities)
        {
            base.OnCapabilitiesChanged(network, networkCapabilities);
            owner.OnCapabilitiesChanged(network, networkCapabilities);
        }

        public override void OnLost(AndroidNetwork network)
        {
            base.OnLost(network);
            owner.OnLost(network);
        }
    }

    // ------------------------------------------------------------
    // Process
    // ------------------------------------------------------------

    private partial void ReadProcessStat(out int threadCount, out long workingSet)
    {
        threadCount = 0;
        workingSet = 0;

        Span<byte> buffer = stackalloc byte[StatBufferSize];
        var values = buffer[..RandomAccess.Read(statHandle, buffer, 0)];

        values = values[(values.LastIndexOf((byte)')') + 2)..];
        for (var field = 3; field <= ResidentPagesField; field++)
        {
            var end = values.IndexOf((byte)' ');
            var value = end < 0 ? values : values[..end];
            if (field == ThreadCountField)
            {
                threadCount = Utf8Parser.TryParse(value, out int count, out _) ? count : 0;
            }
            else if (field == ResidentPagesField)
            {
                workingSet = Utf8Parser.TryParse(value, out long pages, out _) ? pages * PageSize : 0;
            }

            if (end < 0)
            {
                break;
            }

            values = values[(end + 1)..];
        }
    }
}
