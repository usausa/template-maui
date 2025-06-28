namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;

public sealed partial class DeviceMiscViewModel : AppViewModelBase
{
    private readonly IScreen screen;

    private readonly ISpeechService speech;

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

    public IObserveCommand BrightnessCommand { get; }

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
        this.speech = speech;

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

        BrightnessCommand = MakeDelegateCommand<float>(screen.SetScreenBrightness);

        ScreenshotCommand = MakeAsyncCommand(async () =>
        {
            await using var stream = await screen.TakeScreenshotAsync();
            await using var file = File.Create(Path.Combine(storage.PublicFolder, "screenshot.jpg"));
            await stream.CopyToAsync(file);
        });

        SpeakCommand = MakeDelegateCommand(() =>
        {
#pragma warning disable CA2012
            _ = speech.SpeakAsync("テストです");
#pragma warning restore CA2012
        });
        SpeakCancelCommand = MakeDelegateCommand(speech.SpeakCancel);
        Disposables.Add(speech.RecognizedAsObservable().ObserveOnCurrentContext().Subscribe(x => RecognizeText = x.Text));
        RecognizeCommand = MakeAsyncCommand(async () =>
        {
            RecognizeText = string.Empty;
            await speech.RecognizeAsync(CultureInfo.CurrentCulture);
        });
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        screen.SetOrientation(DisplayOrientation.Portrait);

        speech.RecognizeCancel();

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
