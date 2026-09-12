namespace Template.MobileApp.Modules.UI;

public sealed class UIStreamPoster
{
    public string Image { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public string Badge { get; init; } = string.Empty;

    public bool HasDuration => Duration.Length > 0;
    public bool HasBadge => Badge.Length > 0;
}

public sealed class UIStreamSection
{
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<UIStreamPoster> Items { get; init; } = [];
    public int Delay { get; init; }
}

public sealed partial class UIStreamViewModel : AppViewModelBase
{
    public string HeroBadge { get; } = "NEW SEASON";
    public string HeroTitle { get; } = "君の知らない空の果てで";
    public string HeroMeta { get; } = "2024 · SF · 2h 18m";
    public string HeroRating { get; } = "★ 8.4";
    public string HeroRatingSub { get; } = "本日の高評価";

    [ObservableProperty]
    public partial bool InMyList { get; set; }

    public IReadOnlyList<UIStreamSection> Sections { get; }

    public IObserveCommand DetailCommand { get; }

    public IObserveCommand MyListCommand { get; }

    public UIStreamViewModel()
    {
        DetailCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIStreamDetail));
        MyListCommand = MakeDelegateCommand(() => InMyList = !InMyList);

        var posters = new[]
        {
            new UIStreamPoster { Image = "poster01.jpg", Title = "星海のリング", Duration = "1h 42m", Badge = "NEW" },
            new UIStreamPoster { Image = "poster02.jpg", Title = "紅の残響", Duration = "2h 05m" },
            new UIStreamPoster { Image = "poster03.jpg", Title = "屋上の約束", Badge = "LIVE" },
            new UIStreamPoster { Image = "poster04.jpg", Title = "キッチン三人組", Duration = "58m" },
            new UIStreamPoster { Image = "poster05.jpg", Title = "山の記憶", Duration = "1h 12m" }
        };

        Sections =
        [
            new() { Title = "高評価", Items = posters, Delay = 0 },
            new() { Title = "オリジナル", Items = posters, Delay = 80 },
            new() { Title = "急上昇", Items = posters, Delay = 160 },
            new() { Title = "アクション & アドベンチャー", Items = posters, Delay = 240 }
        ];
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
