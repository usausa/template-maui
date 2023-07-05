#pragma warning disable SA1135
namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components.Device;

public class DeviceInfoViewModel : AppViewModelBase
{
    public NotificationValue<Version> DeviceVersion { get; } = new();
    public NotificationValue<string> DeviceName { get; } = new();
    public NotificationValue<bool> IsDeviceEmulator { get; } = new();

    public NotificationValue<string> ApplicationName { get; } = new();
    public NotificationValue<string> ApplicationPackageName { get; } = new();
    public NotificationValue<Version> ApplicationVersion { get; } = new();
    public NotificationValue<string> ApplicationBuild { get; } = new();

    public DeviceInfoViewModel(
        ApplicationState applicationState,
        IDeviceManager device)
        : base(applicationState)
    {
        DeviceVersion.Value = device.DeviceVersion;
        DeviceName.Value = device.DeviceName;
        IsDeviceEmulator.Value = device.IsDeviceEmulator;

        ApplicationName.Value = device.ApplicationName;
        ApplicationPackageName.Value = device.ApplicationPackageName;
        ApplicationVersion.Value = device.ApplicationVersion;
        ApplicationBuild.Value = device.ApplicationBuild;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
