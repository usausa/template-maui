namespace Template.MobileApp.Modules.Sample;

using System.Collections.ObjectModel;
using System.Net;
using System.Text;

using Microsoft.Extensions.AI;

using OllamaSharp;
using OllamaSharp.Models.Exceptions;

using Template.MobileApp.Models.Sample.Chat;

using AiMessage = Microsoft.Extensions.AI.ChatMessage;

public sealed partial class SampleChatViewModel : AppViewModelBase
{
    private readonly ISpeechService speech;

    private readonly IDispatcher dispatcher;

    private readonly string model;

    private readonly List<AiMessage> history = [];

    private Action? cancelResponse;

    [ObservableProperty]
    public partial string InputText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsListening { get; private set; }

    [ObservableProperty]
    public partial bool IsResponding { get; private set; }

    public ObservableCollection<AiChatMessage> Messages { get; } = [];

    private IChatClient ChatClient { get; }

    public IObserveCommand VoiceCommand { get; }

    public IObserveCommand SendCommand { get; }

    public IObserveCommand CancelCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SampleChatViewModel(
        ISpeechService speech,
        IDispatcher dispatcher,
        Settings settings)
    {
        this.speech = speech;
        this.dispatcher = dispatcher;

        ChatClient = new OllamaApiClient(new Uri(settings.OllamaEndPoint), settings.OllamaModel);
        Disposables.Add(ChatClient);
        model = settings.OllamaModel;

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
                Text = $"こんにちは!AI アシスタントです。開発に関する質問をどうぞ 🤖\n(Ollama: {model})"
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

        history.Add(new AiMessage(ChatRole.User, text));
        var builder = new StringBuilder();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        cancelResponse = cts.Cancel;
        try
        {
            await Task.Run(async () =>
            {
                await foreach (var update in ChatClient.GetStreamingResponseAsync(history, cancellationToken: token).ConfigureAwait(false))
                {
                    builder.Append(update.Text);
                    var current = builder.ToString();
                    await dispatcher.DispatchAsync(() =>
                    {
                        message.IsTyping = false;
                        message.Text = current;
                    }).ConfigureAwait(false);
                }
            }, token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex) when (ex is HttpRequestException or OllamaException or WebException or IOException)
        {
            if (!cts.IsCancellationRequested)
            {
                history.RemoveAt(history.Count - 1);
                message.IsTyping = false;
                message.Text = $"応答を取得できませんでした。\n{ex.Message}";
                return;
            }
        }
        finally
        {
            cancelResponse = null;
        }

        message.IsTyping = false;
        if (!cts.IsCancellationRequested)
        {
            history.Add(new AiMessage(ChatRole.Assistant, builder.ToString()));
        }
        else if (builder.Length > 0)
        {
            message.Text = $"{builder}\n(中断)";
            history.Add(new AiMessage(ChatRole.Assistant, builder.ToString()));
        }
        else
        {
            history.RemoveAt(history.Count - 1);
            message.Text = "中断しました。";
        }
    }
}
