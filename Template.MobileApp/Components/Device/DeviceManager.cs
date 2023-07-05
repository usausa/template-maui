namespace Template.MobileApp.Components.Device;

public interface IDeviceManager
{
    // Status

    IObservable<NetworkState> NetworkState { get; }

    NetworkState GetNetworkState();

    // Display

    Orientation GetOrientation();

    void SetOrientation(Orientation orientation);

    ValueTask<Stream> TakeScreenshotAsync();

    void KeepScreenOn(bool value);

    // Feed

    void Vibrate(double duration);

    void VibrateCancel();

    void FeedbackClick();

    void FeedbackLongPress();

    // Light

    void LightOn();

    void LightOff();

    // Information

    Version DeviceVersion { get; }

    string DeviceName { get; }

    bool IsDeviceEmulator { get; }

    string ApplicationName { get; }

    string ApplicationPackageName { get; }

    Version ApplicationVersion { get; }

    string ApplicationBuild { get; }
}

public sealed partial class DeviceManager : IDeviceManager, IDisposable
{
    private readonly IAppInfo appInfo;

    private readonly IDeviceInfo deviceInfo;

    private readonly IDeviceDisplay deviceDisplay;

    private readonly IVibration vibration;

    private readonly IHapticFeedback feedback;

    private readonly IFlashlight flashlight;

    private readonly IScreenshot screenshot;

    private readonly BehaviorSubject<NetworkState> networkState;

    public IObservable<NetworkState> NetworkState => networkState;

    public DeviceManager(
        IAppInfo appInfo,
        IDeviceInfo deviceInfo,
        IDeviceDisplay deviceDisplay,
        IVibration vibration,
        IHapticFeedback feedback,
        IFlashlight flashlight,
        IScreenshot screenshot)
    {
        this.appInfo = appInfo;
        this.deviceInfo = deviceInfo;
        this.deviceDisplay = deviceDisplay;
        this.vibration = vibration;
        this.feedback = feedback;
        this.flashlight = flashlight;
        this.screenshot = screenshot;

        networkState = new BehaviorSubject<NetworkState>(GetNetworkState(Connectivity.NetworkAccess, Connectivity.ConnectionProfiles));
        Connectivity.ConnectivityChanged += (_, args) =>
        {
            networkState.OnNext(GetNetworkState(args.NetworkAccess, args.ConnectionProfiles));
        };
    }

    public void Dispose()
    {
        networkState.Dispose();
    }

    // ------------------------------------------------------------
    // Status
    // ------------------------------------------------------------

    private static NetworkState GetNetworkState(NetworkAccess access, IEnumerable<ConnectionProfile> profiles)
    {
        if (access != NetworkAccess.None && access != NetworkAccess.Unknown)
        {
            return profiles.Any(x => x == ConnectionProfile.Ethernet || x == ConnectionProfile.WiFi)
                ? Template.MobileApp.Components.Device.NetworkState.ConnectedHighSpeed
                : Template.MobileApp.Components.Device.NetworkState.Connected;
        }

        return Template.MobileApp.Components.Device.NetworkState.Disconnected;
    }

    public NetworkState GetNetworkState() => GetNetworkState(Connectivity.NetworkAccess, Connectivity.ConnectionProfiles);

    // ------------------------------------------------------------
    // Display
    // ------------------------------------------------------------

    public Orientation GetOrientation() =>
        deviceDisplay.MainDisplayInfo.Orientation switch
        {
            DisplayOrientation.Landscape => Orientation.Landscape,
            DisplayOrientation.Portrait => Orientation.Portrait,
            _ => Orientation.Unknown
        };

    public async ValueTask<Stream> TakeScreenshotAsync()
    {
        var result = await screenshot.CaptureAsync();
        return await result.OpenReadAsync();
    }

    public void KeepScreenOn(bool value) => deviceDisplay.KeepScreenOn = value;

    // ------------------------------------------------------------
    // Feed
    // ------------------------------------------------------------

    public void Vibrate(double duration) => vibration.Vibrate(duration);

    public void VibrateCancel() => vibration.Cancel();

    public void FeedbackClick() => feedback.Perform(HapticFeedbackType.Click);

    public void FeedbackLongPress() => feedback.Perform(HapticFeedbackType.LongPress);

    // ------------------------------------------------------------
    // Light
    // ------------------------------------------------------------

    public void LightOn() => flashlight.TurnOnAsync();

    public void LightOff() => flashlight.TurnOffAsync();

    // ------------------------------------------------------------
    // Information
    // ------------------------------------------------------------

    public Version DeviceVersion => deviceInfo.Version;

    public string DeviceName => deviceInfo.Name;

    public bool IsDeviceEmulator => deviceInfo.DeviceType == DeviceType.Virtual;

    public string ApplicationName => appInfo.Name;

    public string ApplicationPackageName => appInfo.PackageName;

    public Version ApplicationVersion => appInfo.Version;

    public string ApplicationBuild => appInfo.BuildString;
}
