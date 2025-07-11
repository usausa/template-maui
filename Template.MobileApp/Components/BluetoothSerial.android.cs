namespace Template.MobileApp.Components;

using Android.Bluetooth;
using Android.Content;
using Android.Util;

using Java.Util;

public sealed partial class BluetoothSerialFactory
{
    private static readonly UUID SppUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB")!;

    private readonly Context context;

    private readonly BluetoothAdapter adapter;

    public BluetoothSerialFactory()
    {
        context = ActivityResolver.CurrentActivity.ApplicationContext!;
        var bluetoothManager = (BluetoothManager)context.GetSystemService(Context.BluetoothService)!;
        adapter = bluetoothManager.Adapter!;
    }

    public partial async ValueTask<IBluetoothSerial?> ConnectAsync(string name, byte[]? pin)
    {
        var status = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<Microsoft.Maui.ApplicationModel.Permissions.Bluetooth>();
        if (status != PermissionStatus.Granted)
        {
            status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.Bluetooth>();
            if (status != PermissionStatus.Granted)
            {
                return null;
            }
        }

        // Find
        var device = await FindAsync(name);
        if (device is null)
        {
            return null;
        }

        var socket = default(BluetoothSocket?);
        try
        {
            // Bond
            if (device.BondState != Bond.Bonded)
            {
                if (!await BondAsync(device, pin ?? []))
                {
                    device.Dispose();
                    return null;
                }
            }

            socket = device.CreateRfcommSocketToServiceRecord(SppUuid);
            if (socket is null)
            {
                return null;
            }

            await socket.ConnectAsync();

            return new BluetoothSerial(socket);
        }
        catch (Java.Lang.Throwable ex)
        {
            Log.Error(nameof(BluetoothSerialFactory), ex, "Unknown exception.");
            socket?.Dispose();
            device.Dispose();
            return null;
        }
    }

    private async ValueTask<BluetoothDevice?> FindAsync(string name)
    {
        // Find
        var tcs = new TaskCompletionSource<BluetoothDevice?>();

        using var receiver = new FindReceiver(tcs, name);
        using var filter = new IntentFilter();
        filter.AddAction(BluetoothDevice.ActionFound);
        filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
        context.RegisterReceiver(receiver, filter);

        if (!adapter.StartDiscovery())
        {
            context.UnregisterReceiver(receiver);
            return null;
        }

        var device = await tcs.Task;

        adapter.CancelDiscovery();

        context.UnregisterReceiver(receiver);

        return device;
    }

    private async ValueTask<bool> BondAsync(BluetoothDevice device, byte[] pin)
    {
        var tcs = new TaskCompletionSource<bool>();

        using var receiver = new BondReceiver(tcs, pin);
        using var filter = new IntentFilter();
        filter.AddAction(BluetoothDevice.ActionPairingRequest);
        filter.AddAction(BluetoothDevice.ActionBondStateChanged);
        //filter.Priority = (int)IntentFilterPriority.HighPriority;
        context.RegisterReceiver(receiver, filter);

        // Timeout
        //var cts = new CancellationTokenSource(10_000);
        //cts.Token.Register(() => tcs.TrySetResult(false));

        if (!device.CreateBond())
        {
            context.UnregisterReceiver(receiver);
            return false;
        }

        var result = await tcs.Task;

        //cts.Dispose();
        context.UnregisterReceiver(receiver);

        return result;
    }

    // ------------------------------------------------------------
    // Receiver
    // ------------------------------------------------------------

    private sealed class FindReceiver : BroadcastReceiver
    {
        private readonly TaskCompletionSource<BluetoothDevice?> tcs;

        private readonly string name;

        public FindReceiver(TaskCompletionSource<BluetoothDevice?> tcs, string name)
        {
            this.tcs = tcs;
            this.name = name;
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            switch (intent!.Action)
            {
                case BluetoothDevice.ActionFound:
#pragma warning disable CA1422
                    var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice)!;
#pragma warning restore CA1422
                    Log.Debug(nameof(BluetoothSerialFactory), $"[BluetoothDevice.ActionFound] {device.Name}");
                    if ((device.Name is not null) && device.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    {
                        tcs.TrySetResult(device);
                    }
                    break;

                case BluetoothAdapter.ActionDiscoveryFinished:
                    Log.Debug(nameof(BluetoothSerialFactory), "[BluetoothAdapter.ActionDiscoveryFinished]");
                    tcs.TrySetResult(null);
                    break;
            }
        }
    }

    private sealed class BondReceiver : BroadcastReceiver
    {
        private readonly TaskCompletionSource<bool> tcs;

        private readonly byte[] pin;

        public BondReceiver(TaskCompletionSource<bool> tcs, byte[] pin)
        {
            this.tcs = tcs;
            this.pin = pin;
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            switch (intent!.Action)
            {
                case BluetoothDevice.ActionPairingRequest:
#pragma warning disable CA1422
                    var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice)!;
#pragma warning restore CA1422
                    Log.Debug(nameof(BluetoothSerialFactory), $"[BluetoothDevice.ActionPairingRequest] {device.Name}");
                    if (pin.Length > 0)
                    {
                        device.SetPin(pin);
                    }
                    break;

                case BluetoothDevice.ActionBondStateChanged:
                    var state = (Bond)intent.GetIntExtra(BluetoothDevice.ExtraBondState, BluetoothDevice.Error);
                    var previousState = (Bond)intent.GetIntExtra(BluetoothDevice.ExtraPreviousBondState, BluetoothDevice.Error);
                    Log.Debug(nameof(BluetoothSerialFactory), $"[BluetoothDevice.ActionBondStateChanged] {previousState} -> {state}");
                    if (state == Bond.Bonded)
                    {
                        tcs.TrySetResult(true);
                    }
                    else if (state == Bond.None)
                    {
                        tcs.TrySetResult(false);
                    }
                    break;
            }
        }
    }

    // ------------------------------------------------------------
    // BluetoothSerial
    // ------------------------------------------------------------

    // ReSharper disable once UnusedType.Local
    private sealed class BluetoothSerial : IBluetoothSerial
    {
        private readonly BluetoothSocket socket;

        public Stream Input => socket.InputStream!;

        public Stream Output => socket.OutputStream!;

        public BluetoothSerial(BluetoothSocket socket)
        {
            this.socket = socket;
        }

        public void Dispose()
        {
            socket.Close();
        }
    }
}
