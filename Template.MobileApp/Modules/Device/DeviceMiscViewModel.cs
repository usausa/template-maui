#pragma warning disable SA1135
namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components.Device;
using Template.MobileApp.Components.Speech;
using Template.MobileApp.Components.Storage;

public class DeviceMiscViewModel : AppViewModelBase
{
    private readonly IDeviceManager device;

    public ICommand KeepScreenOnCommand { get; }
    public ICommand KeepScreenOffCommand { get; }

    public ICommand OrientationPortraitCommand { get; }
    public ICommand OrientationLandscapeCommand { get; }

    public ICommand VibrateCommand { get; }
    public ICommand VibrateCancelCommand { get; }

    public ICommand LightOnCommand { get; }
    public ICommand LightOffCommand { get; }

    public ICommand ScreenshotCommand { get; }

    public ICommand SpeakCommand { get; }
    public ICommand SpeakCancelCommand { get; }

    public ICommand RecognizeCommand { get; }

    public NotificationValue<string> RecognizeText { get; } = new();

    public DeviceMiscViewModel(
        ApplicationState applicationState,
        IDeviceManager device,
        IStorageManager storage,
        ISpeechManager speech)
        : base(applicationState)
    {
        this.device = device;

        KeepScreenOnCommand = MakeDelegateCommand(() => device.KeepScreenOn(true));
        KeepScreenOffCommand = MakeDelegateCommand(() => device.KeepScreenOn(false));

        OrientationPortraitCommand = MakeDelegateCommand(() => device.SetOrientation(Orientation.Portrait));
        OrientationLandscapeCommand = MakeDelegateCommand(() => device.SetOrientation(Orientation.Landscape));

        VibrateCommand = MakeDelegateCommand(() => device.Vibrate(5000));
        VibrateCancelCommand = MakeDelegateCommand(device.VibrateCancel);

        LightOnCommand = MakeDelegateCommand(device.LightOn);
        LightOffCommand = MakeDelegateCommand(device.LightOff);

        ScreenshotCommand = MakeAsyncCommand(async () =>
        {
            await using var stream = await device.TakeScreenshotAsync();
            await using var file = File.Create(Path.Combine(storage.PublicFolder, "screenshot.jpg"));
            await stream.CopyToAsync(file);
        });

#pragma warning disable CA2012
        SpeakCommand = MakeDelegateCommand(() => speech.SpeakAsync("テストです"));
#pragma warning restore CA2012
        SpeakCancelCommand = MakeDelegateCommand(speech.SpeakCancel);

        RecognizeCommand = MakeAsyncCommand(async () =>
        {
            RecognizeText.Value = string.Empty;

            var result = await speech.RecognizeAsync(text => RecognizeText.Value += text);

            RecognizeText.Value = !String.IsNullOrEmpty(result) ? result : string.Empty;
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    public override void OnNavigatingFrom(INavigationContext context)
    {
        device.SetOrientation(Orientation.Portrait);
    }
}
