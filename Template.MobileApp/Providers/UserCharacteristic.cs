namespace Template.MobileApp.Providers;

using Shiny.BluetoothLE.Hosting;

public sealed class UserCharacteristic
{
    private readonly Guid guid;

    public UserCharacteristic(Settings settings)
    {
        guid = Guid.Parse(settings.UniqueId);
    }

    public Task Register(IBleHostingManager manager) =>
        manager.AddService(BleConstants.UserServiceUuid, true, service =>
            service.AddCharacteristic(BleConstants.UserCharacteristicUuid, characteristic =>
                characteristic.SetRead(_ => Task.FromResult(GattResult.Success(guid.ToByteArray())))));
}
