namespace Template.MobileApp.Components;

using Android.Bluetooth;
using Android.Content;
using Android.Util;

using AndroidX.Core.Content;

using Java.Util;

public sealed partial class BluetoothSerialFactory
{
    private static readonly UUID SppUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB")!;

    private static readonly TimeSpan DiscoveryTimeout = TimeSpan.FromSeconds(30);

    // ペアリング確認ダイアログのユーザー操作を考慮した値
    private static readonly TimeSpan BondTimeout = TimeSpan.FromSeconds(60);

    private readonly Context context;

    private readonly BluetoothAdapter? adapter;

    public partial bool IsSupported => adapter is not null;

    public BluetoothSerialFactory()
    {
        context = ActivityResolver.CurrentActivity.ApplicationContext!;
        var bluetoothManager = (BluetoothManager?)context.GetSystemService(Context.BluetoothService);
        adapter = bluetoothManager?.Adapter;
    }

    public async partial ValueTask<IBluetoothSerial?> ConnectAsync(string name, byte[]? pin)
    {
        if (adapter is null)
        {
            return null;
        }

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
        var device = await FindAsync(adapter, name);
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

    private async ValueTask<BluetoothDevice?> FindAsync(BluetoothAdapter adapter, string name)
    {
        // Find
        var tcs = new TaskCompletionSource<BluetoothDevice?>();

        using var receiver = new FindReceiver(tcs, name);
        using var filter = new IntentFilter();
        filter.AddAction(BluetoothDevice.ActionFound);
        filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
        ContextCompat.RegisterReceiver(context, receiver, filter, ContextCompat.ReceiverNotExported);
        try
        {
            if (!adapter.StartDiscovery())
            {
                return null;
            }

            using var cts = new CancellationTokenSource(DiscoveryTimeout);
            return await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        finally
        {
            adapter.CancelDiscovery();
            context.UnregisterReceiver(receiver);
        }
    }

    private async ValueTask<bool> BondAsync(BluetoothDevice device, byte[] pin)
    {
        var tcs = new TaskCompletionSource<bool>();

        using var receiver = new BondReceiver(tcs, pin);
        using var filter = new IntentFilter();
        filter.AddAction(BluetoothDevice.ActionPairingRequest);
        filter.AddAction(BluetoothDevice.ActionBondStateChanged);
        ContextCompat.RegisterReceiver(context, receiver, filter, ContextCompat.ReceiverNotExported);
        try
        {
            if (!device.CreateBond())
            {
                return false;
            }

            using var cts = new CancellationTokenSource(BondTimeout);
            return await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            context.UnregisterReceiver(receiver);
        }
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
            socket.Dispose();
        }
    }
}
