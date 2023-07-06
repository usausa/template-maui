#pragma warning disable SA1135
namespace Template.MobileApp.Modules.Device;

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
        IDeviceInfo deviceInfo,
        IAppInfo appInfo)
        : base(applicationState)
    {
        DeviceVersion.Value = deviceInfo.Version;
        DeviceName.Value = deviceInfo.Name;
        IsDeviceEmulator.Value = deviceInfo.DeviceType == DeviceType.Virtual;

        ApplicationName.Value = appInfo.Name;
        ApplicationPackageName.Value = appInfo.PackageName;
        ApplicationVersion.Value = appInfo.Version;
        ApplicationBuild.Value = appInfo.BuildString;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
