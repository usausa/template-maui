namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components.Storage;

public class DeviceMiscViewModel : AppViewModelBase
{
    private readonly IScreen screen;

    public ICommand KeepScreenOnCommand { get; }
    public ICommand KeepScreenOffCommand { get; }

    public ICommand OrientationPortraitCommand { get; }
    public ICommand OrientationLandscapeCommand { get; }

    public ICommand VibrateCommand { get; }
    public ICommand VibrateCancelCommand { get; }

    public ICommand FeedbackClickCommand { get; }
    public ICommand FeedbackLongPressCommand { get; }

    public ICommand LightOnCommand { get; }
    public ICommand LightOffCommand { get; }

    public ICommand ScreenshotCommand { get; }

    public ICommand SpeakCommand { get; }
    public ICommand SpeakCancelCommand { get; }

    public ICommand RecognizeCommand { get; }

    public NotificationValue<string> RecognizeText { get; } = new();

    public DeviceMiscViewModel(
        ApplicationState applicationState,
        IScreen screen,
        IStorageManager storage,
        ISpeechService speech,
        IVibration vibration,
        IHapticFeedback feedback,
        IFlashlight flashlight)
        : base(applicationState)
    {
        this.screen = screen;

        KeepScreenOnCommand = MakeDelegateCommand(() => screen.KeepScreenOn(true));
        KeepScreenOffCommand = MakeDelegateCommand(() => screen.KeepScreenOn(false));

        OrientationPortraitCommand = MakeDelegateCommand(() => screen.SetOrientation(DisplayOrientation.Portrait));
        OrientationLandscapeCommand = MakeDelegateCommand(() => screen.SetOrientation(DisplayOrientation.Landscape));

        VibrateCommand = MakeDelegateCommand(() => vibration.Vibrate(5000));
        VibrateCancelCommand = MakeDelegateCommand(vibration.Cancel);

        FeedbackClickCommand = MakeDelegateCommand(() => feedback.Perform(HapticFeedbackType.Click));
        FeedbackLongPressCommand = MakeDelegateCommand(() => feedback.Perform(HapticFeedbackType.LongPress));

        LightOnCommand = MakeAsyncCommand(flashlight.TurnOnAsync);
        LightOffCommand = MakeAsyncCommand(flashlight.TurnOffAsync);

        ScreenshotCommand = MakeAsyncCommand(async () =>
        {
            await using var stream = await screen.TakeScreenshotAsync();
            await using var file = File.Create(Path.Combine(storage.PublicFolder, "screenshot.jpg"));
            await stream.CopyToAsync(file);
        });

#pragma warning disable CA2012
        SpeakCommand = MakeDelegateCommand(() => speech.SpeakAsync("テストです"));
#pragma warning restore CA2012
        SpeakCancelCommand = MakeDelegateCommand(speech.SpeakCancel);

        var progress = new Progress<string>(text =>
        {
            if (!String.IsNullOrEmpty(text))
            {
                RecognizeText.Value = text;
            }
        });
        RecognizeCommand = MakeAsyncCommand(async () =>
        {
            RecognizeText.Value = string.Empty;

            var result = await speech.RecognizeAsync(progress);

            RecognizeText.Value = !String.IsNullOrEmpty(result) ? result : string.Empty;
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    public override void OnNavigatingFrom(INavigationContext context)
    {
        screen.SetOrientation(DisplayOrientation.Portrait);
    }
}
