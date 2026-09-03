namespace Template.MobileApp.Modules.Sample;

using System.Collections.ObjectModel;

using Template.MobileApp.Models.Sample.Chat;

// 音声フローの抽出プレビュー項目
public sealed record VoiceExtractItem(string Label, string Value);

public sealed partial class SampleChatViewModel : AppViewModelBase
{
    private static readonly (string Text, bool IsCode)[] Replies =
    [
        ("なるほど、良い質問ですね。.NET MAUI では XAML でレイアウトを宣言し、データバインディングで ViewModel と接続します。コードビハインドを使わずに Behavior や Trigger で振る舞いを追加するのがおすすめです。", false),
        ("その場合は BindableProperty を定義してコントロールに公開します。例を書いてみますね。", false),
        ("public sealed class GreetingService\n{\n    public string CreateMessage(string name)\n    {\n        ArgumentNullException.ThrowIfNull(name);\n        return $\"Hello, {name}! Welcome to .NET MAUI.\";\n    }\n}", true),
        ("補足すると、リスト表示には CollectionView を使い、ItemsUpdatingScrollMode を KeepLastItemInView にするとチャットのように末尾へ追従します。パフォーマンスが必要な場面では DataTemplateSelector でテンプレートを分けるのが定石です。", false),
    ];

    // 音声フロー (C-7)。録音と AI 処理はモックで UI パターンのみ再現する
    private const string MockTranscript = "明日の15時までにログイン画面の不具合修正をお願いします。再現手順は共有済みのチケットを参照してください。";

    private readonly IDispatcherTimer recordTimer;

    private int replyIndex;

    private bool responding;

    [ObservableProperty]
    public partial string InputText { get; set; } = string.Empty;

    public ObservableCollection<AiChatMessage> Messages { get; } = [];

    public IObserveCommand SendCommand { get; }

    // ------------------------------------------------------------------ 音声フロー

    [ObservableProperty]
    public partial bool VoiceVisible { get; private set; }

    [ObservableProperty(NotifyAlso = [nameof(IsStep1), nameof(IsStep2), nameof(IsStep3), nameof(IsStep4)])]
    public partial int VoiceStep { get; private set; } = 1;

    public bool IsStep1 => VoiceStep == 1;

    public bool IsStep2 => VoiceStep == 2;

    public bool IsStep3 => VoiceStep == 3;

    public bool IsStep4 => VoiceStep == 4;

    [ObservableProperty]
    public partial bool IsRecording { get; private set; }

    [ObservableProperty]
    public partial int RecordSeconds { get; private set; }

    [ObservableProperty]
    public partial bool Transcribing { get; private set; }

    [ObservableProperty]
    public partial string TranscribedText { get; private set; } = string.Empty;

    public IReadOnlyList<VoiceExtractItem> ExtractItems { get; } =
    [
        new("種別", "依頼"),
        new("期日", "明日 15:00"),
        new("対象", "ログイン画面の不具合修正"),
        new("参照", "共有済みのチケット")
    ];

    public IObserveCommand OpenVoiceCommand { get; }
    public IObserveCommand CloseVoiceCommand { get; }
    public IObserveCommand ToggleRecordCommand { get; }
    public IObserveCommand GoExtractCommand { get; }
    public IObserveCommand GoApproveCommand { get; }
    public IObserveCommand ApplyVoiceCommand { get; }
    public IObserveCommand RetryVoiceCommand { get; }

    public SampleChatViewModel(IDispatcher dispatcher)
    {
        SendCommand = MakeAsyncCommand(SendAsync, () => !responding && !String.IsNullOrWhiteSpace(InputText));

        recordTimer = dispatcher.CreateTimer();
        recordTimer.Interval = TimeSpan.FromSeconds(1);
        Disposables.Add(recordTimer.TickAsObservable().Subscribe(_ => RecordSeconds++));

        OpenVoiceCommand = MakeDelegateCommand(() =>
        {
            ResetVoice();
            VoiceVisible = true;
        });
        CloseVoiceCommand = MakeDelegateCommand(CloseVoice);
        ToggleRecordCommand = MakeAsyncCommand(ToggleRecordAsync);
        GoExtractCommand = MakeDelegateCommand(() => VoiceStep = 3, () => !Transcribing);
        GoApproveCommand = MakeDelegateCommand(() => VoiceStep = 4);
        ApplyVoiceCommand = MakeDelegateCommand(() =>
        {
            InputText = TranscribedText;
            CloseVoice();
        });
        RetryVoiceCommand = MakeDelegateCommand(ResetVoice);

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(InputText))
            {
                SendCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName == nameof(Transcribing))
            {
                GoExtractCommand.RaiseCanExecuteChanged();
            }
        };
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (Messages.Count == 0)
        {
            Messages.Add(new AiChatMessage
            {
                Role = AiChatRole.Assistant,
                Text = "こんにちは!AI アシスタントです。開発に関する質問をどうぞ 🤖",
            });
        }
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        CloseVoice();
        return Task.CompletedTask;
    }

    private async Task SendAsync()
    {
        var text = InputText.Trim();
        InputText = string.Empty;
        Messages.Add(new AiChatMessage { Role = AiChatRole.User, Text = text });

        responding = true;
        SendCommand.RaiseCanExecuteChanged();
        try
        {
            var (reply, isCode) = Replies[replyIndex % Replies.Length];
            replyIndex++;

            // タイピングインジケータを表示してから応答をストリーミング風に流し込む
            var message = new AiChatMessage { Role = AiChatRole.Assistant, IsCode = isCode, IsTyping = true };
            Messages.Add(message);

            await Task.Delay(1200).ConfigureAwait(true);
            message.IsTyping = false;

            for (var i = 0; i < reply.Length; i += 3)
            {
                message.Text = reply[..Math.Min(i + 3, reply.Length)];
                await Task.Delay(30).ConfigureAwait(true);
            }
        }
        finally
        {
            responding = false;
            SendCommand.RaiseCanExecuteChanged();
        }
    }

    private async Task ToggleRecordAsync()
    {
        if (!IsRecording)
        {
            IsRecording = true;
            RecordSeconds = 0;
            recordTimer.Start();
            return;
        }

        IsRecording = false;
        recordTimer.Stop();

        // 文字起こし (モック)。少し待ってから固定文を表示する
        VoiceStep = 2;
        Transcribing = true;
        TranscribedText = string.Empty;
        await Task.Delay(1500).ConfigureAwait(true);
        if (VoiceVisible)
        {
            TranscribedText = MockTranscript;
            Transcribing = false;
        }
    }

    private void ResetVoice()
    {
        recordTimer.Stop();
        IsRecording = false;
        RecordSeconds = 0;
        Transcribing = false;
        TranscribedText = string.Empty;
        VoiceStep = 1;
    }

    private void CloseVoice()
    {
        ResetVoice();
        VoiceVisible = false;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
