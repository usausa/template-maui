namespace Template.MobileApp.Components.Device;

public interface IDeviceManager
{
    public void SetScreenBrightness(float brightness);
}

public sealed partial class DeviceManager : IDeviceManager
{
    public partial void SetScreenBrightness(float brightness);
}
