namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;

public sealed partial class DeviceMiscViewModel : AppViewModelBase
{
    private const int NotificationId = 1;

    private const int ScheduledNotificationId = 2;

    private readonly IScreen screen;

    private readonly ISpeechService speech;

    [ObservableProperty]
    public partial double SpeechRate { get; set; } = 1.0;

    [ObservableProperty]
    public partial string RecognizeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsListening { get; set; }

    [ObservableProperty]
    public partial string NotificationText { get; set; } = string.Empty;

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

    public IObserveCommand NotifyCommand { get; }
    public IObserveCommand NotifyScheduleCommand { get; }
    public IObserveCommand NotifyCancelCommand { get; }
    public IObserveCommand NotifyExactSettingCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DeviceMiscViewModel(
        IScreen screen,
        ISpeechService speech,
        IVibration vibration,
        IHapticFeedback feedback,
        IFlashlight flashlight,
        IStorageManager storage,
        INotificationService notification)
    {
        this.screen = screen;
        this.speech = speech;

        KeepScreenOnCommand = MakeDelegateCommand(() => screen.KeepScreenOn(true));
        KeepScreenOffCommand = MakeDelegateCommand(() => screen.KeepScreenOn(false));

        OrientationPortraitCommand = MakeDelegateCommand(() => screen.SetOrientation(DisplayOrientation.Portrait));
        OrientationLandscapeCommand = MakeDelegateCommand(() => screen.SetOrientation(DisplayOrientation.Landscape));

        VibrateCommand = MakeDelegateCommand(() => vibration.Vibrate(5000), () => vibration.IsSupported);
        VibrateCancelCommand = MakeDelegateCommand(vibration.Cancel, () => vibration.IsSupported);

        FeedbackClickCommand = MakeDelegateCommand(() => feedback.Perform(HapticFeedbackType.Click), () => feedback.IsSupported);
        FeedbackLongPressCommand = MakeDelegateCommand(() => feedback.Perform(HapticFeedbackType.LongPress), () => feedback.IsSupported);

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
            _ = speech.SpeakAsync("テストです", rate: (float)SpeechRate);
#pragma warning restore CA2012
        });
        SpeakCancelCommand = MakeDelegateCommand(speech.SpeakCancel);
        Disposables.Add(speech.RecognizedAsObservable().ObserveOnCurrentContext().Subscribe(x =>
        {
            if (!String.IsNullOrEmpty(x.Text))
            {
                RecognizeText = x.Text;
            }

            if (x.Complete)
            {
                IsListening = false;
            }
        }));
        RecognizeCommand = MakeAsyncCommand(async () =>
        {
            if (IsListening)
            {
                await speech.RecognizeStopAsync();
                return;
            }

            RecognizeText = string.Empty;
            IsListening = true;
            if (!await speech.RecognizeAsync(CultureInfo.CurrentCulture))
            {
                IsListening = false;
            }
        });

        NotifyCommand = MakeAsyncCommand(async () =>
        {
            if (!await Permissions.RequestNotificationsAsync())
            {
                NotificationText = "通知の許可がありません";
                return;
            }

            notification.Show(NotificationId, "承認依頼", "SO-2026-000123 の承認をお願いします", "SO-2026-000123", [new("approve", "承認"), new("reject", "却下")]);
            NotificationText = "通知を表示しました (本体またはボタンのタップで戻ります)";
        });
        NotifyScheduleCommand = MakeAsyncCommand(async () =>
        {
            if (!await Permissions.RequestNotificationsAsync())
            {
                NotificationText = "通知の許可がありません";
                return;
            }

            notification.Schedule(ScheduledNotificationId, "リマインダー", "10 秒後に予約した通知です", TimeSpan.FromSeconds(10), "reminder");
            NotificationText = notification.CanScheduleExact ? "10 秒後に通知します" : "10 秒後に通知します (正確なアラームが未許可のため前後します)";
        });
        NotifyCancelCommand = MakeDelegateCommand(() =>
        {
            notification.Cancel(NotificationId);
            notification.Cancel(ScheduledNotificationId);
            NotificationText = "通知を取り消しました";
        });
        NotifyExactSettingCommand = MakeDelegateCommand(notification.OpenExactAlarmSettings);
        Disposables.Add(notification.TappedAsObservable().ObserveOnCurrentContext().Subscribe(x =>
            NotificationText = x.Action is null ? $"タップ: {x.Payload}" : $"ボタン [{x.Action}]: {x.Payload}"));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatingFromAsync(INavigationContext context)
    {
        screen.SetOrientation(DisplayOrientation.Portrait);

        IsListening = false;
        await speech.RecognizeCancelAsync();
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
