namespace Template.MobileApp.Components;

public interface IDeviceManager
{
    public void SetScreenBrightness(float brightness);
}

public sealed partial class DeviceManager : IDeviceManager
{
    public partial void SetScreenBrightness(float brightness);
}
