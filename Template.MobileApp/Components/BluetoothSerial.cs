namespace Template.MobileApp.Components;

public interface IBluetoothSerial : IDisposable
{
    Stream Input { get; }

    Stream Output { get; }
}

public interface IBluetoothSerialFactory
{
    // 非搭載端末ではfalse。falseの場合ConnectAsyncは常にnullを返す
    bool IsSupported { get; }

    ValueTask<IBluetoothSerial?> ConnectAsync(string name, byte[]? pin = null);
}

public sealed partial class BluetoothSerialFactory : IBluetoothSerialFactory
{
    public partial bool IsSupported { get; }

    public partial ValueTask<IBluetoothSerial?> ConnectAsync(string name, byte[]? pin = null);
}
