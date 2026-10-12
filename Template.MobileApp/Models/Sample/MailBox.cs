namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum MailFolder
{
    Inbox,
    Sent,
    Draft,
    Trash
}

// 一覧で見るフォルダー (スター付きは、ゴミ箱以外のスターの付いたメール)
public enum MailView
{
    Inbox,
    Starred,
    Sent,
    Draft,
    Trash
}

public enum MailDay
{
    Today,
    Yesterday,
    ThisWeek,
    Earlier
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class MailMessage : ObservableObject
{
    public DateTime DateTime { get; set; }

    // 差出人の画像のファイル名 (無いときは頭文字のアイコン)
    public string? Avatar { get; set; }

    public string From { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Body { get; set; } = default!;

    // 添付のファイル名
    public string? Attachment { get; set; }

    public bool HasAttachment => Attachment is not null;

    // 一覧の要約 (改行を詰める)
    public string Preview => string.Join(' ', Body.Split('\n', StringSplitOptions.RemoveEmptyEntries));

    [ObservableProperty]
    public partial MailFolder Folder { get; set; }

    [ObservableProperty]
    public partial bool IsUnread { get; set; }

    [ObservableProperty]
    public partial bool IsStarred { get; set; }
}

// 日付の区切り (今日・昨日・今週・それ以前) ごとのメール
public sealed class MailGroup : ReadOnlyCollection<MailMessage>
{
    public MailDay Day { get; }

    public MailGroup(MailDay day, IList<MailMessage> items)
        : base(items)
    {
        Day = day;
    }
}

// フォルダーの件数 (受信トレイは未読の数)
public sealed partial class MailViewCount : ObservableObject
{
    public required MailView View { get; init; }

    [ObservableProperty]
    public partial int Count { get; set; }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

// メールボックス (フォルダーごとの一覧と数、既読・スター・削除・アーカイブ)
public sealed class MailBox
{
    private readonly List<MailMessage> messages;

    public IReadOnlyList<MailViewCount> Counts { get; } =
        Enum.GetValues<MailView>().Select(static x => new MailViewCount { View = x }).ToArray();

    public MailBox(IEnumerable<MailMessage> messages)
    {
        this.messages = [.. messages];
        UpdateCounts();
    }

    // フォルダーのメールを新しい順に、日付で区切る
    public IReadOnlyList<MailGroup> Find(MailView view, DateTime now) =>
        messages
            .Where(x => Match(x, view))
            .OrderByDescending(static x => x.DateTime)
            .GroupBy(x => DayOf(x.DateTime, now))
            .Select(static x => new MailGroup(x.Key, x.ToList()))
            .ToList();

    public void Read(MailMessage message)
    {
        message.IsUnread = false;
        UpdateCounts();
    }

    public void ToggleStar(MailMessage message)
    {
        message.IsStarred = !message.IsStarred;
        UpdateCounts();
    }

    // ゴミ箱の中では消す。ほかはゴミ箱へ
    public void Delete(MailMessage message)
    {
        if (message.Folder == MailFolder.Trash)
        {
            messages.Remove(message);
        }
        else
        {
            message.Folder = MailFolder.Trash;
        }
        UpdateCounts();
    }

    public void Archive(MailMessage message)
    {
        messages.Remove(message);
        UpdateCounts();
    }

    private void UpdateCounts()
    {
        foreach (var count in Counts)
        {
            count.Count = count.View == MailView.Inbox
                ? messages.Count(static x => (x.Folder == MailFolder.Inbox) && x.IsUnread)
                : messages.Count(x => Match(x, count.View));
        }
    }

    private static bool Match(MailMessage message, MailView view) => view switch
    {
        MailView.Inbox => message.Folder == MailFolder.Inbox,
        MailView.Starred => message.IsStarred && (message.Folder != MailFolder.Trash),
        MailView.Sent => message.Folder == MailFolder.Sent,
        MailView.Draft => message.Folder == MailFolder.Draft,
        _ => message.Folder == MailFolder.Trash
    };

    // 今日・昨日・今週 (7 日以内)・それ以前
    private static MailDay DayOf(DateTime date, DateTime now) =>
        (now.Date - date.Date).Days switch
        {
            <= 0 => MailDay.Today,
            1 => MailDay.Yesterday,
            < 7 => MailDay.ThisWeek,
            _ => MailDay.Earlier
        };
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// メールの見本 (日時は今からの相対)
public static class MailSample
{
    // ReSharper disable StringLiteralTypo
    public static IReadOnlyList<MailMessage> LoadMails(DateTime now)
    {
        var today = now.Date;
        return
        [
            new MailMessage
            {
                DateTime = now.AddMinutes(-25),
                Avatar = "mofusand.jpg",
                From = "山奥通信",
                Title = "【ご案内】定例会議について",
                Body = $"各位\n\nお疲れさまです。山奥通信の事務局です。\n今月の定例会議を次のとおり開きます。\n\n日時: {today.AddDays(3):M/d} 14:00〜15:00\n場所: 3 階 会議室 A(オンラインでも参加できます)\n議題: 下期の計画と各チームの進み具合\n\n資料を添付しますので、事前にご確認ください。",
                Attachment = "定例会議_資料.pdf",
                IsUnread = true,
                IsStarred = true
            },
            new MailMessage
            {
                DateTime = now.AddHours(-2),
                Avatar = "genbaneko.png",
                From = "現場猫bot",
                Title = "作業前安全確認(本日の点検項目)",
                Body = "おはようございます。本日の作業前点検です。\n\n・ヘルメットとあご紐\n・安全帯のフック\n・脚立の開き止め\n・足元の整理整頓\n\n指差し確認、ヨシ!",
                IsUnread = true
            },
            new MailMessage
            {
                DateTime = now.AddHours(-3).AddMinutes(-40),
                From = "うさぎ急便",
                Title = "お荷物のお届け予定のお知らせ",
                Body = $"いつもうさぎ急便をご利用いただきありがとうございます。\n\nお問い合わせ番号: 4821-0937-5521\nお届け予定: {today.AddDays(1):M/d} 18:00〜20:00\n\nお届けの日時は、このメールのリンクから変更できます。",
                IsUnread = true
            },
            new MailMessage
            {
                DateTime = today.AddDays(-1).AddHours(18).AddMinutes(42),
                Avatar = "usausa.png",
                From = "うさうさ・メープル・フレンチトースト",
                Title = "Re: 週末の打ち合わせについて",
                Body = "お疲れさまです、うさうさです。\n\n土曜の打ち合わせは 13 時からで大丈夫です。\n場所は駅前のカフェ モカにしましょう。\n\n当日の資料は前の日までに共有します。"
            },
            new MailMessage
            {
                DateTime = today.AddDays(-1).AddHours(10).AddMinutes(5),
                From = "クラウド請求センター",
                Title = $"【ご請求】{today.AddMonths(-1).Month}月分のご利用料金のお知らせ",
                Body = $"{today.AddMonths(-1).Month}月分のご利用料金が確定しました。\n\nご請求額: 12,480 円(税込)\nお支払い日: {today.Month}月27日\n\n明細は添付の PDF をご確認ください。",
                Attachment = $"invoice_{today.AddMonths(-1):yyyyMM}.pdf",
                IsStarred = true
            },
            new MailMessage
            {
                DateTime = today.AddDays(-3).AddHours(15).AddMinutes(30),
                Avatar = "mofusand.jpg",
                From = "山奥通信",
                Title = $"社内報 {today.Month}月号を公開しました",
                Body = "社内報の最新号を公開しました。\n\n今月の特集は「新しいオフィスの一日」です。各部署の取り組みや、社員のおすすめのランチも紹介しています。"
            },
            new MailMessage
            {
                DateTime = today.AddDays(-5).AddHours(9),
                Avatar = "genbaneko.png",
                From = "現場猫bot",
                Title = "安全大会の開催について",
                Body = "来週の安全大会のお知らせです。\n\n日時: 来週の月曜 9:00〜\n内容: ヒヤリハットの共有と、保護具の点検\n\n各班の代表は 1 件以上の事例を用意してください。"
            },
            new MailMessage
            {
                DateTime = today.AddDays(-10).AddHours(16).AddMinutes(20),
                From = "経理部 佐藤",
                Title = "経費精算の締め切りについて",
                Body = "経理部の佐藤です。\n\n今月の経費精算の締め切りは 25 日です。領収書の画像を忘れずに添付してください。"
            },
            new MailMessage
            {
                DateTime = today.AddDays(-20).AddHours(21).AddMinutes(3),
                Avatar = "usausa.png",
                From = "うさうさ・メープル・フレンチトースト",
                Title = "先日はありがとうございました",
                Body = "先日は勉強会でお世話になりました。\nいただいたサンプルのコードを、さっそく試してみます。\nまた次回もよろしくお願いします。"
            },
            new MailMessage
            {
                DateTime = today.AddDays(-1).AddHours(19).AddMinutes(10),
                Avatar = "mofusand.jpg",
                From = "山奥通信",
                Title = "議事録を共有します",
                Body = "先日の会議の議事録を共有します。ご確認ください。",
                Attachment = "議事録.docx",
                Folder = MailFolder.Sent
            },
            new MailMessage
            {
                DateTime = today.AddDays(-4).AddHours(11).AddMinutes(30),
                Avatar = "usausa.png",
                From = "うさうさ・メープル・フレンチトースト",
                Title = "週末の打ち合わせについて",
                Body = "土曜の午後に打ち合わせできますか?",
                Folder = MailFolder.Sent
            },
            new MailMessage
            {
                DateTime = today.AddDays(-2).AddHours(17),
                From = "経理部 佐藤",
                Title = "Re: 経費精算の締め切りについて",
                Body = "承知しました。今週中に申請します。",
                Folder = MailFolder.Draft
            },
            new MailMessage
            {
                DateTime = today.AddDays(-12).AddHours(8).AddMinutes(30),
                Avatar = "genbaneko.png",
                From = "現場猫bot",
                Title = "作業前安全確認",
                Body = "今日も一日ゼロ災でいきましょう。",
                Folder = MailFolder.Trash
            }
        ];
    }
    // ReSharper restore StringLiteralTypo
}
