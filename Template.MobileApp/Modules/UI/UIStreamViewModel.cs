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
    public string HeroTitle { get; } = "Galactic Riders";
    public string HeroMeta { get; } = "2024 · Sci-Fi · 2h 18m";
    public string HeroRating { get; } = "★ 8.4";
    public string HeroRatingSub { get; } = "Top rated today";

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
            new UIStreamPoster { Image = "social_background.png", Title = "Title 1", Duration = "1h 42m", Badge = "NEW" },
            new UIStreamPoster { Image = "social_background.png", Title = "Title 2", Duration = "2h 05m" },
            new UIStreamPoster { Image = "social_background.png", Title = "Title 3", Badge = "LIVE" },
            new UIStreamPoster { Image = "social_background.png", Title = "Title 4", Duration = "58m" },
            new UIStreamPoster { Image = "social_background.png", Title = "Title 5", Duration = "1h 12m" }
        };

        Sections =
        [
            new() { Title = "Top Rated", Items = posters, Delay = 0 },
            new() { Title = "Originals", Items = posters, Delay = 80 },
            new() { Title = "Trending Now", Items = posters, Delay = 160 },
            new() { Title = "Action & Adventure", Items = posters, Delay = 240 }
        ];
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
