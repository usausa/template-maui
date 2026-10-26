namespace Template.MobileApp.Modules.Sample;

using System.Collections.ObjectModel;

using Template.MobileApp.Usecase;

public sealed partial class SampleChatViewModel : AppViewModelBase
{
    // Field

    private readonly ISpeechService speech;

    private Action? cancelResponse;

    // Property

    [ObservableProperty]
    public partial string InputText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsListening { get; private set; }

    [ObservableProperty]
    public partial bool IsResponding { get; private set; }

    public ObservableCollection<AiChatMessage> Messages { get; } = [];

    // A field would be reported by CA2213, the session is disposed through Disposables
    private ChatSession Session { get; }

    // Command

    public IObserveCommand VoiceCommand { get; }

    public IObserveCommand SendCommand { get; }

    public IObserveCommand CancelCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SampleChatViewModel(
        ISpeechService speech,
        ChatUsecase chatUsecase)
    {
        this.speech = speech;

        Session = chatUsecase.CreateSession();
        Disposables.Add(Session);

        VoiceCommand = MakeAsyncCommand(ToggleVoiceAsync);
        SendCommand = MakeDelegateCommand(() => _ = SendAsync(), () => !IsResponding && !String.IsNullOrWhiteSpace(InputText));
        CancelCommand = MakeDelegateCommand(() => cancelResponse?.Invoke(), () => IsResponding);

        Disposables.Add(speech.RecognizedAsObservable().ObserveOnCurrentContext().Subscribe(x =>
        {
            if (!IsListening)
            {
                return;
            }

            if (!String.IsNullOrEmpty(x.Text))
            {
                InputText = x.Text;
            }

            if (x.Complete)
            {
                IsListening = false;
            }
        }));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Messages.Add(new AiChatMessage
            {
                Role = AiChatRole.Assistant,
                Text = $"こんにちは!AIアシスタントです。開発に関する質問をどうぞ 🤖\n(Ollama: {Session.Model})"
            });
        }
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        cancelResponse?.Invoke();
        return CancelVoiceAsync();
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private async Task ToggleVoiceAsync()
    {
        if (IsListening)
        {
            await speech.RecognizeStopAsync();
            return;
        }

        if (!await Permissions.RequestMicrophoneAsync())
        {
            return;
        }

        IsListening = true;
        if (!await speech.RecognizeAsync(CultureInfo.CurrentCulture))
        {
            IsListening = false;
        }
    }

    private async Task CancelVoiceAsync()
    {
        if (IsListening)
        {
            IsListening = false;
            await speech.RecognizeCancelAsync();
        }
    }

    private async Task SendAsync()
    {
        await CancelVoiceAsync();

        var text = InputText.Trim();
        InputText = string.Empty;
        Messages.Add(new AiChatMessage { Role = AiChatRole.User, Text = text });

        IsResponding = true;
        try
        {
            await RespondAsync(text);
        }
        finally
        {
            IsResponding = false;
        }
    }

    private async Task RespondAsync(string text)
    {
        var message = new AiChatMessage { Role = AiChatRole.Assistant, IsTyping = true };
        Messages.Add(message);

        using var cts = new CancellationTokenSource();
        cancelResponse = cts.Cancel;
        try
        {
            var answer = await Session.SendAsync(
                text,
                new Progress<string>(x =>
                {
                    message.IsTyping = false;
                    message.Text = x;
                }),
                cts.Token);

            message.IsTyping = false;
            message.Text = answer switch
            {
                { Error: { } error } => $"応答を取得できませんでした。\n{error}",
                { Canceled: true, Text.Length: 0 } => "中断しました。",
                { Canceled: true } => $"{answer.Text}\n(中断)",
                _ => answer.Text
            };
        }
        finally
        {
            cancelResponse = null;
        }
    }
}
