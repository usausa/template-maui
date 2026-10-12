namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum MessageType
{
    Send,
    Receive,
    System
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class MessageReaction : ObservableObject
{
    public string Emoji { get; set; } = default!;

    [ObservableProperty]
    public partial int Count { get; set; }
}

public sealed class ChatMessage : ObservableObject
{
    public MessageType Type { get; set; }

    public DateTime DateTime { get; init; } = DateTime.Now;

    public string Author { get; set; } = default!;

    public string TextContent { get; set; } = default!;

    public string? AvatarSource { get; set; }

    public string? StampSource { get; set; }

    public string? PhotoSource { get; set; }

    public bool IsRead { get; set; }

    public ObservableCollection<MessageReaction> Reactions { get; init; } = [];

    // 同じ絵文字があれば数を足し、無ければ 1 で加える
    public void AddReaction(string emoji)
    {
        var reaction = Reactions.FirstOrDefault(x => x.Emoji == emoji);
        if (reaction is not null)
        {
            reaction.Count++;
            return;
        }

        Reactions.Add(new MessageReaction { Emoji = emoji, Count = 1 });
        RaisePropertyChanged(nameof(Reactions));
    }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

// トーク。自分のメッセージ・写真・スタンプを作る
public static class ChatTalk
{
    private const string Me = "自分";

    private const string AvatarMe = "avatar_person05.jpg";

    public static ChatMessage Send(
        DateTime dateTime, string text, bool isRead = false,
        IReadOnlyList<MessageReaction>? reactions = null) =>
        new()
        {
            Type = MessageType.Send,
            DateTime = dateTime,
            Author = Me,
            AvatarSource = AvatarMe,
            TextContent = text,
            IsRead = isRead,
            Reactions = [.. reactions ?? []]
        };

    public static ChatMessage SendPhoto(DateTime dateTime, string photo) =>
        new()
        {
            Type = MessageType.Send,
            DateTime = dateTime,
            Author = Me,
            AvatarSource = AvatarMe,
            PhotoSource = photo,
            TextContent = string.Empty
        };

    public static ChatMessage SendStamp(DateTime dateTime, string stamp, bool isRead = false) =>
        new()
        {
            Type = MessageType.Send,
            DateTime = dateTime,
            Author = Me,
            AvatarSource = AvatarMe,
            StampSource = stamp,
            TextContent = string.Empty,
            IsRead = isRead
        };
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// トークの見本 (昨日と今日の会話・スタンプの一覧)
public static class ChatSample
{
    private const string AvatarAlice = "avatar_person01.jpg";
    private const string AvatarBob = "avatar_person02.jpg";
    private const string AvatarCarol = "avatar_person03.jpg";
    private const string AvatarDave = "avatar_person04.jpg";

    private static readonly string[] Stamps =
    [
        "stamp01.png", "stamp02.png", "stamp03.png", "stamp04.png",
        "stamp05.png", "stamp06.png", "stamp07.png", "stamp08.png"
    ];

    public static IReadOnlyList<string> LoadStamps() => Stamps;

    public static IReadOnlyList<ChatMessage> LoadMessages(DateTime today)
    {
        var yesterday = today.AddDays(-1);
        return
        [
            DateDivider(yesterday),

            Receive(yesterday.AddHours(9).AddMinutes(5), "M･I･O", AvatarAlice, "おはようございます。"),
            ChatTalk.Send(yesterday.AddHours(9).AddMinutes(18), "おはようございます。", isRead: true),
            Receive(yesterday.AddHours(9).AddMinutes(30), "日本酒飲郎", AvatarBob, "昨日の PR レビューしました。CI が通っていないようなのでテストの修正をお願いできますか？コメントもいくつか書いてあります。"),
            ChatTalk.Send(yesterday.AddHours(9).AddMinutes(32), "ありがとうございます！\n午前中に対応します。", isRead: true),
            Receive(yesterday.AddHours(12).AddMinutes(30), "日本酒飲郎", AvatarBob, "お昼ご飯食べてきます〜", reactions: [new MessageReaction { Emoji = "🍱", Count = 3 }]),
            Receive(yesterday.AddHours(14), "悪いスライム", AvatarCarol, "定例始めます。"),
            ChatTalk.Send(yesterday.AddHours(14).AddMinutes(1), "入ります。", isRead: true),
            Receive(yesterday.AddHours(16), "M･I･O", AvatarAlice, "資料 PDF 共有しますね。"),
            ChatTalk.Send(yesterday.AddHours(16).AddMinutes(5), "確認しました！", isRead: true, reactions: [new MessageReaction { Emoji = "🙏", Count = 1 }]),
            Receive(yesterday.AddHours(18).AddMinutes(30), "†聖天使†", AvatarDave, "お疲れさまでした！"),

            DateDivider(today),

            Receive(today.AddHours(10).AddMinutes(5), "M･I･O", AvatarAlice, "資料できましたー！来週の会議で使うものなので、月曜日までに確認をお願いします🙏"),
            ChatTalk.Send(today.AddHours(10).AddMinutes(7), "了解しました！\n以下の点を確認します。\n・議事録\n・来週の資料\n・レビュー", isRead: true),
            ReceiveStamp(today.AddHours(10).AddMinutes(10), "日本酒飲郎", AvatarBob, StampOf(0)),
            ChatTalk.Send(today.AddHours(10).AddMinutes(12), "👀 確認中…", isRead: true, reactions: [new MessageReaction { Emoji = "👀", Count = 1 }]),
            ChatTalk.SendStamp(today.AddHours(10).AddMinutes(15), StampOf(1), isRead: true),
            Receive(today.AddHours(10).AddMinutes(30), "悪いスライム", AvatarCarol, "今日は 15:00 から会議です。"),
            ReceiveStamp(today.AddHours(10).AddMinutes(35), "悪いスライム", AvatarCarol, StampOf(2)),
            ChatTalk.Send(today.AddHours(10).AddMinutes(37), "了解しました。", isRead: true),
            ChatTalk.SendStamp(today.AddHours(10).AddMinutes(40), StampOf(3), isRead: true),
            Receive(today.AddHours(11), "M･I･O", AvatarAlice, "ランチ何にします？"),
            ReceiveStamp(today.AddHours(11).AddMinutes(1), "M･I･O", AvatarAlice, StampOf(4)),
            Receive(today.AddHours(11).AddMinutes(2), "日本酒飲郎", AvatarBob, "寿司でどうでしょう。", reactions: [new MessageReaction { Emoji = "🍣", Count = 2 }]),
            ReceiveStamp(today.AddHours(11).AddMinutes(3), "日本酒飲郎", AvatarBob, StampOf(5)),
            ChatTalk.Send(today.AddHours(11).AddMinutes(5), "いいですね！ちなみに本日のミーティングお疲れさまでした。共有いただいた資料についていくつか質問があるので、後ほど別途連絡いたします。", isRead: false),
            ChatTalk.SendStamp(today.AddHours(11).AddMinutes(6), StampOf(6), isRead: false)
        ];
    }

    // 日付の区切り (文言は画面で日付から作る)
    private static ChatMessage DateDivider(DateTime date) =>
        new()
        {
            Type = MessageType.System,
            DateTime = date,
            TextContent = string.Empty
        };

    private static ChatMessage Receive(
        DateTime dateTime, string author, string avatar, string text,
        IReadOnlyList<MessageReaction>? reactions = null) =>
        new()
        {
            Type = MessageType.Receive,
            DateTime = dateTime,
            Author = author,
            AvatarSource = avatar,
            TextContent = text,
            Reactions = [.. reactions ?? []]
        };

    private static ChatMessage ReceiveStamp(DateTime dateTime, string author, string avatar, string stamp) =>
        new()
        {
            Type = MessageType.Receive,
            DateTime = dateTime,
            Author = author,
            AvatarSource = avatar,
            StampSource = stamp,
            TextContent = string.Empty
        };

    private static string StampOf(int index) => Stamps[index % Stamps.Length];
}
