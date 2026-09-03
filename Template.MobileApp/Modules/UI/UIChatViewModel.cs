namespace Template.MobileApp.Modules.UI;

using System.Collections.ObjectModel;

using Template.MobileApp.Models.Sample.Chat;

public sealed partial class UIChatViewModel : AppViewModelBase
{
    private const string AvatarAlice = "usa1_face.jpg";
    private const string AvatarBob = "usa2_face.jpg";
    private const string AvatarCarol = "usa3_face.jpg";
    private const string AvatarDave = "usa4_face.jpg";
    private const string AvatarMe = "usa5_face.jpg";

    private static readonly string[] Stamps =
    [
        "stamp01.png", "stamp02.png", "stamp03.png", "stamp04.png",
        "stamp05.png", "stamp06.png", "stamp07.png", "stamp08.png"
    ];

    private static string GetStamp(int index) => Stamps[index % Stamps.Length];

    private readonly IDispatcher dispatcher;

    public CollectionController Controller { get; } = new();

    public ObservableCollection<ChatMessage> Messages { get; } = [];

    public IReadOnlyList<string> StampList { get; } = Stamps;

    public string CurrentUser { get; } = "Me";

    [ObservableProperty]
    public partial string InputText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsStampTrayVisible { get; set; }

    public IObserveCommand SendCommand { get; }
    public IObserveCommand SendStampCommand { get; }
    public IObserveCommand PickImageCommand { get; }
    public IObserveCommand PickStickerCommand { get; }
    public IObserveCommand ScrollToLatestCommand { get; }

    public UIChatViewModel(IDispatcher dispatcher)
    {
        this.dispatcher = dispatcher;

        SendCommand = MakeDelegateCommand(ExecuteSend, () => !String.IsNullOrWhiteSpace(InputText));
        SendStampCommand = MakeDelegateCommand<string>(ExecuteSendStamp);
        PickImageCommand = MakeDelegateCommand(static () => { });
        PickStickerCommand = MakeDelegateCommand(() => IsStampTrayVisible = !IsStampTrayVisible);
        ScrollToLatestCommand = MakeDelegateCommand(() => ScrollToLast());
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(InputText))
            {
                SendCommand.RaiseCanExecuteChanged();
            }
        };
    }

    private void ScrollToLast(bool animate = true)
    {
        if (Messages.Count == 0)
        {
            return;
        }

        // 追加直後はレイアウト前のため次のループでスクロールする
        dispatcher.Dispatch(() => Controller.ScrollRequest(Messages.Count - 1, position: ScrollToPosition.End, animate: animate));
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (Messages.Count == 0)
        {
            LoadSampleMessages();
        }
        ScrollToLast(animate: false);
        return Task.CompletedTask;
    }

    private void ExecuteSend()
    {
        Messages.Add(new ChatMessage
        {
            Type = MessageType.Send,
            DateTime = DateTime.Now,
            Author = CurrentUser,
            AvatarSource = AvatarMe,
            TextContent = InputText.Trim()
        });
        InputText = string.Empty;
        IsStampTrayVisible = false;
        ScrollToLast();
    }

    private void ExecuteSendStamp(string stamp)
    {
        Messages.Add(new ChatMessage
        {
            Type = MessageType.Send,
            DateTime = DateTime.Now,
            Author = CurrentUser,
            AvatarSource = AvatarMe,
            StampSource = stamp,
            TextContent = string.Empty
        });
        IsStampTrayVisible = false;
        ScrollToLast();
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    private void LoadSampleMessages()
    {
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);

        AddSystem(yesterday);

        AddReceive(yesterday.AddHours(9).AddMinutes(5), "Alice", AvatarAlice, "おはようございます。");
        AddSend(yesterday.AddHours(9).AddMinutes(18), "おはようございます。", isRead: true);
        AddReceive(yesterday.AddHours(9).AddMinutes(30), "Bob", AvatarBob,
            "昨日の PR レビューしました。CI が通っていないようなのでテストの修正をお願いできますか？コメントもいくつか書いてあります。");
        AddSend(yesterday.AddHours(9).AddMinutes(32), "ありがとうございます！\n午前中に対応します。", isRead: true);
        AddReceive(yesterday.AddHours(12).AddMinutes(30), "Bob", AvatarBob, "お昼ご飯食べてきます〜",
            reactions: [new MessageReaction { Emoji = "🍱", Count = 3 }]);
        AddReceive(yesterday.AddHours(14), "Carol", AvatarCarol, "定例始めます。");
        AddSend(yesterday.AddHours(14).AddMinutes(1), "入ります。", isRead: true);
        AddReceive(yesterday.AddHours(16), "Alice", AvatarAlice, "資料 PDF 共有しますね。");
        AddSend(yesterday.AddHours(16).AddMinutes(5), "確認しました！", isRead: true,
            reactions: [new MessageReaction { Emoji = "🙏", Count = 1 }]);
        AddReceive(yesterday.AddHours(18).AddMinutes(30), "Dave", AvatarDave, "お疲れさまでした！");

        AddSystem(today);

        AddReceive(today.AddHours(10).AddMinutes(5), "Alice", AvatarAlice,
            "資料できましたー！来週の会議で使うものなので、月曜日までに確認をお願いします🙏");
        AddSend(today.AddHours(10).AddMinutes(7),
            "了解しました！\n以下の点を確認します。\n・議事録\n・来週の資料\n・レビュー", isRead: true);
        AddReceiveStamp(today.AddHours(10).AddMinutes(10), "Bob", AvatarBob, GetStamp(0));
        AddSend(today.AddHours(10).AddMinutes(12), "👀 確認中…", isRead: true,
            reactions: [new MessageReaction { Emoji = "👀", Count = 1 }]);
        AddSendStamp(today.AddHours(10).AddMinutes(15), GetStamp(1), isRead: true);
        AddReceive(today.AddHours(10).AddMinutes(30), "Carol", AvatarCarol, "今日は 15:00 から会議です。");
        AddReceiveStamp(today.AddHours(10).AddMinutes(35), "Carol", AvatarCarol, GetStamp(2));
        AddSend(today.AddHours(10).AddMinutes(37), "了解しました。", isRead: true);
        AddSendStamp(today.AddHours(10).AddMinutes(40), GetStamp(3), isRead: true);
        AddReceive(today.AddHours(11), "Alice", AvatarAlice, "ランチ何にします？");
        AddReceiveStamp(today.AddHours(11).AddMinutes(1), "Alice", AvatarAlice, GetStamp(4));
        AddReceive(today.AddHours(11).AddMinutes(2), "Bob", AvatarBob, "寿司でどうでしょう。",
            reactions: [new MessageReaction { Emoji = "🍣", Count = 2 }]);
        AddReceiveStamp(today.AddHours(11).AddMinutes(3), "Bob", AvatarBob, GetStamp(5));
        AddSend(today.AddHours(11).AddMinutes(5),
            "いいですね！ちなみに本日のミーティングお疲れさまでした。共有いただいた資料についていくつか質問があるので、後ほど別途連絡いたします。",
            isRead: false);
        AddSendStamp(today.AddHours(11).AddMinutes(6), GetStamp(6), isRead: false);
    }

    private void AddSystem(DateTime date) =>
        Messages.Add(new ChatMessage
        {
            Type = MessageType.System,
            DateTime = date,
            TextContent = date.ToString("yyyy年M月d日 (ddd)")
        });

    private void AddReceive(
        DateTime dateTime, string author, string avatar, string text,
        IReadOnlyList<MessageReaction>? reactions = null) =>
        Messages.Add(new ChatMessage
        {
            Type = MessageType.Receive,
            DateTime = dateTime,
            Author = author,
            AvatarSource = avatar,
            TextContent = text,
            Reactions = reactions ?? []
        });

    private void AddReceiveStamp(DateTime dateTime, string author, string avatar, string stampSource) =>
        Messages.Add(new ChatMessage
        {
            Type = MessageType.Receive,
            DateTime = dateTime,
            Author = author,
            AvatarSource = avatar,
            StampSource = stampSource,
            TextContent = string.Empty
        });

    private void AddSendStamp(DateTime dateTime, string stampSource, bool isRead) =>
        Messages.Add(new ChatMessage
        {
            Type = MessageType.Send,
            DateTime = dateTime,
            Author = CurrentUser,
            AvatarSource = AvatarMe,
            StampSource = stampSource,
            TextContent = string.Empty,
            IsRead = isRead
        });

    private void AddSend(
        DateTime dateTime, string text, bool isRead,
        IReadOnlyList<MessageReaction>? reactions = null) =>
        Messages.Add(new ChatMessage
        {
            Type = MessageType.Send,
            DateTime = dateTime,
            Author = CurrentUser,
            AvatarSource = AvatarMe,
            TextContent = text,
            IsRead = isRead,
            Reactions = reactions ?? []
        });
}
