namespace Template.MobileApp.Modules.UI;

public sealed partial class UIKitNotifyItem : ObservableObject
{
    public string Icon { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime Time { get; init; }

    [ObservableProperty]
    public partial bool IsUnread { get; set; }
}

// 日ごとの区切り (今日・昨日・それより前)
public sealed class UIKitNotifyGroup : ObservableCollection<UIKitNotifyItem>
{
    public DateTime Date { get; }

    public UIKitNotifyGroup(DateTime date, IEnumerable<UIKitNotifyItem> items)
        : base(items)
    {
        Date = date;
    }
}

public sealed class UIKitNotifyViewModel : AppViewModelBase
{
    public ObservableCollection<UIKitNotifyGroup> Groups { get; }

    // タップと右へのスワイプで既読、左へのスワイプで削除
    public IObserveCommand ReadCommand { get; }

    public IObserveCommand DeleteCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIKitNotifyViewModel()
    {
        var now = DateTime.Now;
        var eta = KitSample.LoadActivity(now).OrderEta;
        UIKitNotifyItem[] items =
        [
            new() { Icon = Fonts.MaterialIcons.Local_offer, Title = "本日限定 50% オフ", Description = "週末セールは今夜まで。タップして商品を見る。", Time = now.AddMinutes(-5), IsUnread = true },
            new() { Icon = Fonts.MaterialIcons.Local_shipping, Title = "ご注文の商品を配達中です", Description = $"到着予定: {eta:H:mm}", Time = now.AddHours(-1), IsUnread = true },
            new() { Icon = Fonts.MaterialIcons.Mail, Title = "うさうささんから新着メッセージ", Description = "明日の打ち合わせ、予定どおりで大丈夫？", Time = now.AddHours(-3), IsUnread = true },
            new() { Icon = Fonts.MaterialIcons.Directions_walk, Title = "目標の歩数を達成しました", Description = "昨日は 10,000 歩を歩きました。", Time = now.Date.AddDays(-1).AddHours(21) },
            new() { Icon = Fonts.MaterialIcons.Star, Title = "バッジを獲得しました", Description = "今月の注文が 10 件に達しました。", Time = now.Date.AddDays(-1).AddHours(12) },
            new() { Icon = Fonts.MaterialIcons.Notifications, Title = "週間サマリー", Description = "今週の達成状況を確認しましょう。", Time = now.Date.AddDays(-2).AddHours(9) },
            new() { Icon = Fonts.MaterialIcons.Lock, Title = "パスワードを変更しました", Description = "心当たりが無いときは、すぐにお問い合わせください。", Time = now.Date.AddDays(-4).AddHours(19) }
        ];
        Groups = [.. items.GroupBy(static x => x.Time.Date).Select(static x => new UIKitNotifyGroup(x.Key, x))];

        ReadCommand = MakeDelegateCommand<UIKitNotifyItem>(static x => x.IsUnread = false);
        DeleteCommand = MakeDelegateCommand<UIKitNotifyItem>(Delete);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIKitDash);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    // すべて既読
    protected override Task OnNotifyFunction4()
    {
        foreach (var item in Groups.SelectMany(static x => x))
        {
            item.IsUnread = false;
        }
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    // 空になった日の区切りも消す
    private void Delete(UIKitNotifyItem item)
    {
        var group = Groups.FirstOrDefault(x => x.Contains(item));
        if (group is not null)
        {
            group.Remove(item);
            if (group.Count == 0)
            {
                Groups.Remove(group);
            }
        }
    }
}
