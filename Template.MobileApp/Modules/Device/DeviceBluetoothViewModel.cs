namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;
using Template.MobileApp.Helpers;

public enum BluetoothPrintState
{
    Idle,
    Connecting,
    Printing,
    Completed,
    Failed
}

public sealed partial class DeviceBluetoothViewModel : AppViewModelBase
{
    private readonly IBluetoothSerialFactory bluetoothSerialFactory;

    [ObservableProperty(NotifyAlso = [nameof(IsBusy)])]
    public partial BluetoothPrintState State { get; set; }

    [ObservableProperty]
    public partial string Detail { get; set; }

    public bool IsBusy => State is BluetoothPrintState.Connecting or BluetoothPrintState.Printing;

    public IObserveCommand PrintCommand { get; }

    public DeviceBluetoothViewModel(
        IBluetoothSerialFactory bluetoothSerialFactory)
    {
        this.bluetoothSerialFactory = bluetoothSerialFactory;

        Detail = "Sends a test line to \"DummyPrinter\"";

        PrintCommand = MakeAsyncCommand(ExecutePrint, () => !IsBusy);
    }

    private async Task ExecutePrint()
    {
        if (!bluetoothSerialFactory.IsSupported)
        {
            State = BluetoothPrintState.Failed;
            Detail = "Bluetooth is not supported on this device.";
            return;
        }

        State = BluetoothPrintState.Connecting;
        Detail = "Connecting to \"DummyPrinter\"...";

        // 外部IFのIO例外は発生が予期されるため個別にcatchし、Stateを確実に復帰させる
        try
        {
            using var port = await bluetoothSerialFactory.ConnectAsync("DummyPrinter");
            if (port is null)
            {
                State = BluetoothPrintState.Failed;
                Detail = "Failed to connect.";
                return;
            }

            State = BluetoothPrintState.Printing;
            Detail = "Sending test data...";

            // Printing
            await using var lwr = new LineReaderWriter(port.Input, port.Output);

            await lwr.WriteLineAsync("Test");

            var response = await lwr.ReadLineAsync();
            if (response is not "OK")
            {
                State = BluetoothPrintState.Failed;
                Detail = "Failed to read response.";
                return;
            }

            State = BluetoothPrintState.Completed;
            Detail = "Printed successfully.";
        }
        catch (IOException)
        {
            State = BluetoothPrintState.Failed;
            Detail = "Failed to communicate.";
        }
        catch (Java.Lang.Throwable)
        {
            State = BluetoothPrintState.Failed;
            Detail = "Failed to communicate.";
        }
        finally
        {
            // 上記以外の例外が抜けてもコマンドが永久に無効化されないよう状態を戻す
            if (State is BluetoothPrintState.Connecting or BluetoothPrintState.Printing)
            {
                State = BluetoothPrintState.Failed;
                Detail = "Failed to communicate.";
            }
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
