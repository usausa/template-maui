namespace Template.MobileApp.Modules.UI;

public sealed partial class UIKitNotifyItem : ObservableObject
{
    public string Icon { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Time { get; init; } = string.Empty;

    [ObservableProperty]
    public partial bool IsUnread { get; set; }
}

public sealed class UIKitNotifyViewModel : AppViewModelBase
{
    public IReadOnlyList<UIKitNotifyItem> Notifications { get; } =
    [
        new() { Icon = Fonts.MaterialIcons.Local_offer, Title = "本日限定 50% オフ", Description = "スプリングセールは今夜まで。タップして商品を見る。", Time = "5分前", IsUnread = true },
        new() { Icon = Fonts.MaterialIcons.Local_shipping, Title = "ご注文の商品を配達中です", Description = "到着予定: 14:30 〜 16:00", Time = "1時間前", IsUnread = true },
        new() { Icon = Fonts.MaterialIcons.Mail, Title = "うさうさうさんから新着メッセージ", Description = "明日の打ち合わせ、予定どおりで大丈夫？", Time = "3時間前", IsUnread = true },
        new() { Icon = Fonts.MaterialIcons.Star, Title = "バッジを獲得しました", Description = "今月の注文が 10 件に達しました。", Time = "1日前" },
        new() { Icon = Fonts.MaterialIcons.Notifications, Title = "週間サマリー", Description = "今週の達成状況を確認しましょう。", Time = "2日前" }
    ];

    // タップで既読化
    public IObserveCommand ReadCommand { get; }

    public UIKitNotifyViewModel()
    {
        ReadCommand = MakeDelegateCommand<UIKitNotifyItem>(x => x.IsUnread = false);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIKitDash);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
