namespace Template.MobileApp.Components.Screen;

public interface IScreenManager
{
    DisplayOrientation GetOrientation();

    void SetOrientation(DisplayOrientation orientation);

    ValueTask<Stream> TakeScreenshotAsync();

    void KeepScreenOn(bool value);
}

public sealed partial class ScreenManager : IScreenManager
{
    private readonly IDeviceDisplay deviceDisplay;

    private readonly IScreenshot screenshot;

    public ScreenManager(
        IDeviceDisplay deviceDisplay,
        IScreenshot screenshot)
    {
        this.deviceDisplay = deviceDisplay;
        this.screenshot = screenshot;
    }

    public DisplayOrientation GetOrientation() => deviceDisplay.MainDisplayInfo.Orientation;

    public async ValueTask<Stream> TakeScreenshotAsync()
    {
        var result = await screenshot.CaptureAsync();
        return await result.OpenReadAsync();
    }

    public void KeepScreenOn(bool value) => deviceDisplay.KeepScreenOn = value;
}
