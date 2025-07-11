namespace Template.MobileApp.Components;

public interface IBluetoothSerial : IDisposable
{
    Stream Input { get; }

    Stream Output { get; }
}

public interface IBluetoothSerialFactory
{
    ValueTask<IBluetoothSerial?> ConnectAsync(string name, byte[]? pin = null);
}

public sealed partial class BluetoothSerialFactory : IBluetoothSerialFactory
{
    public partial ValueTask<IBluetoothSerial?> ConnectAsync(string name, byte[]? pin = null);
}
