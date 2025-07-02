namespace Template.MobileApp.Providers;

using Shiny.BluetoothLE.Hosting;
using Shiny.BluetoothLE.Hosting.Managed;

[BleGattCharacteristic(BleConstants.UserServiceUuid, BleConstants.UserCharacteristicUuid)]
public sealed class UserCharacteristic : BleGattCharacteristic
{
    private readonly Guid guid;

    public UserCharacteristic(Settings settings)
    {
        guid = Guid.Parse(settings.UniqId);
    }

    public override Task<GattResult> OnRead(ReadRequest request)
    {
        return Task.FromResult(new GattResult(GattState.Success, guid.ToByteArray()));
    }
}
