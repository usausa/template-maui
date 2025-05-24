#pragma warning disable SA1135
namespace Template.MobileApp.Modules.Device;

public sealed partial class DeviceInfoViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial Version DeviceVersion { get; set; }
    [ObservableProperty]
    public partial string DeviceName { get; set; }
    [ObservableProperty]
    public partial bool IsDeviceEmulator { get; set; }

    [ObservableProperty]
    public partial string ApplicationName { get; set; }
    [ObservableProperty]
    public partial string ApplicationPackageName { get; set; }
    [ObservableProperty]
    public partial Version ApplicationVersion { get; set; }
    [ObservableProperty]
    public partial string ApplicationBuild { get; set; }

    [ObservableProperty]
    public partial double DisplayWidth { get; set; }
    [ObservableProperty]
    public partial double DisplayHeight { get; set; }
    [ObservableProperty]
    public partial double DisplayDensity { get; set; }

    public DeviceInfoViewModel(
        IDeviceInfo deviceInfo,
        IAppInfo appInfo,
        IDeviceDisplay display)
    {
        DeviceVersion = deviceInfo.Version;
        DeviceName = deviceInfo.Name;
        IsDeviceEmulator = deviceInfo.DeviceType == DeviceType.Virtual;

        ApplicationName = appInfo.Name;
        ApplicationPackageName = appInfo.PackageName;
        ApplicationVersion = appInfo.Version;
        ApplicationBuild = appInfo.BuildString;

        DisplayWidth = display.MainDisplayInfo.Width;
        DisplayHeight = display.MainDisplayInfo.Height;
        DisplayDensity = display.MainDisplayInfo.Density;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
