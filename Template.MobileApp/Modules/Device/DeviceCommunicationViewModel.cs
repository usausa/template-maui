namespace Template.MobileApp.Modules.Device;

public sealed class DeviceCommunicationViewModel : AppViewModelBase
{
    public IObserveCommand DialCommand { get; }

    public IObserveCommand SmsCommand { get; }

    public IObserveCommand EmailCommand { get; }

    // ReSharper disable CommentTypo
    public DeviceCommunicationViewModel(
        IPhoneDialer dialer,
        ISms sms,
        IEmail email)
    {
        //  Add to AndroidManifest.xml
        //
        // <queries>
        //   <intent>
        //     <action android:name="android.intent.action.DIAL" />
        //     <data android:scheme="tel"/>
        //   </intent>
        // </queries>
        // <queries>
        //   <intent>
        //     <action android:name="android.intent.action.VIEW" />
        //     <data android:scheme="smsto"/>
        //   </intent>
        // </queries>
        // <queries>
        //   <intent>
        //     <action android:name="android.intent.action.SENDTO" />
        //     <data android:scheme="mailto" />
        //   </intent>
        // </queries>

        DialCommand = MakeDelegateCommand(() => dialer.Open("000-0000-0000"), () => dialer.IsSupported);
        SmsCommand = MakeAsyncCommand(SendSms, () => sms.IsComposeSupported);
        EmailCommand = MakeAsyncCommand(SendEmail, () => email.IsComposeSupported);

        Task SendSms()
        {
            var text = "Hello world.";
            var recipients = new List<string> { "000-0000-0000" };
            var message = new SmsMessage(text, recipients);
            return Sms.Default.ComposeAsync(message);
        }
        Task SendEmail()
        {
            var message = new EmailMessage
            {
                Subject = "Hello world",
                Body = "Hello world.",
                BodyFormat = EmailBodyFormat.PlainText,
                To = ["dummy@example.com"]
            };

            return Email.Default.ComposeAsync(message);
        }
    }
    // ReSharper restore CommentTypo

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
