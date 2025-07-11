namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;
using Template.MobileApp.Helpers;

public sealed class DeviceBluetoothViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IBluetoothSerialFactory bluetoothSerialFactory;

    public IObserveCommand PrintCommand { get; }

    public DeviceBluetoothViewModel(
        IDialog dialog,
        IBluetoothSerialFactory bluetoothSerialFactory)
    {
        this.dialog = dialog;
        this.bluetoothSerialFactory = bluetoothSerialFactory;

        PrintCommand = MakeAsyncCommand(ExecutePrint);
    }

    private async Task ExecutePrint()
    {
        using var loading = dialog.Indicator();

        using var port = await bluetoothSerialFactory.ConnectAsync("DummyPrinter");
        if (port is null)
        {
            await dialog.InformationAsync("Failed to connect.");
            return;
        }

        // Printing
        await using var lwr = new LineReaderWriter(port.Input, port.Output);

        await lwr.WriteLineAsync("Test");

        var response = await lwr.ReadLineAsync();
        if (response is not "OK")
        {
            await dialog.InformationAsync("Failed to read response.");
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
