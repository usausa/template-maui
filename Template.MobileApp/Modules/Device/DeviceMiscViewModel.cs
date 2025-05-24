namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components.Storage;

public sealed partial class DeviceMiscViewModel : AppViewModelBase
{
    private readonly IScreen screen;

    public IObserveCommand KeepScreenOnCommand { get; }
    public IObserveCommand KeepScreenOffCommand { get; }

    public IObserveCommand OrientationPortraitCommand { get; }
    public IObserveCommand OrientationLandscapeCommand { get; }

    public IObserveCommand VibrateCommand { get; }
    public IObserveCommand VibrateCancelCommand { get; }

    public IObserveCommand FeedbackClickCommand { get; }
    public IObserveCommand FeedbackLongPressCommand { get; }

    public IObserveCommand LightOnCommand { get; }
    public IObserveCommand LightOffCommand { get; }

    public IObserveCommand ScreenshotCommand { get; }

    public IObserveCommand SpeakCommand { get; }
    public IObserveCommand SpeakCancelCommand { get; }

    public IObserveCommand RecognizeCommand { get; }

    [ObservableProperty]
    public partial string RecognizeText { get; set; } = string.Empty;

    public DeviceMiscViewModel(
        IScreen screen,
        IStorageManager storage,
        ISpeechService speech,
        IVibration vibration,
        IHapticFeedback feedback,
        IFlashlight flashlight)
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

        // TODO
        RecognizeCommand = MakeDelegateCommand(() => { });
#pragma warning disable CA2012
        SpeakCommand = MakeDelegateCommand(() => speech.SpeakAsync("テストです"));
#pragma warning restore CA2012
        SpeakCancelCommand = MakeDelegateCommand(speech.SpeakCancel);
//        var progress = new Progress<string>(text =>
//        {
//            if (!String.IsNullOrEmpty(text))
//            {
//                RecognizeText.Value = text;
//            }
//        });
//        RecognizeCommand = MakeAsyncCommand(async () =>
//        {
//            RecognizeText.Value = string.Empty;

//            var result = await speech.RecognizeAsync(progress);

//            RecognizeText.Value = !String.IsNullOrEmpty(result) ? result : string.Empty;
//        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    public override void OnNavigatingFrom(INavigationContext context)
    {
        screen.SetOrientation(DisplayOrientation.Portrait);
    }
}
