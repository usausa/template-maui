namespace Template.MobileApp.Modules.UI;

public sealed class UIStreamDetailRelated
{
    public string Image { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
}

public sealed class UIStreamDetailTrailer
{
    public string Image { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
}

public sealed partial class UIStreamDetailViewModel : AppViewModelBase
{
    public string Title { get; } = "Galactic Riders";
    public string Match { get; } = "98% Match";
    public string Meta { get; } = "2024 · Sci-Fi · 2h 18m · TV-14";
    public string Synopsis { get; } = "A team of misfit pilots travel beyond the rim to stop a rogue AI from awakening an ancient interstellar weapon.";
    public string CastLine { get; } = "Cast: Aoi Mori, Ren Sato, Mia Chen · Director: K. Tanaka";

    [ObservableProperty]
    public partial bool TrailersSelected { get; set; } = true;

    [ObservableProperty]
    public partial bool RelatedSelected { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial bool IsDownloaded { get; set; }

    public IObserveCommand SelectTabCommand { get; }

    public IObserveCommand FavoriteCommand { get; }

    public IObserveCommand DownloadCommand { get; }

    public IReadOnlyList<UIStreamDetailTrailer> Trailers { get; } =
    [
        new() { Image = "social_background.png", Title = "Official Trailer", Duration = "Trailer · 2m 14s" },
        new() { Image = "social_background.png", Title = "Teaser: First Flight", Duration = "Teaser · 1m 02s" },
        new() { Image = "social_background.png", Title = "Behind the Scenes", Duration = "Extra · 4m 30s" }
    ];

    public IReadOnlyList<UIStreamDetailRelated> Related { get; } =
    [
        new() { Image = "social_background.png", Title = "Star Drift", Duration = "1h 38m" },
        new() { Image = "social_background.png", Title = "Nebula Run", Duration = "2h 01m" },
        new() { Image = "social_background.png", Title = "Iron Comet", Duration = "1h 45m" },
        new() { Image = "social_background.png", Title = "Void Racer", Duration = "1h 52m" }
    ];

    public UIStreamDetailViewModel()
    {
        SelectTabCommand = MakeDelegateCommand<string>(x =>
        {
            TrailersSelected = x == "Trailers";
            RelatedSelected = !TrailersSelected;
        });
        FavoriteCommand = MakeDelegateCommand(() => IsFavorite = !IsFavorite);
        DownloadCommand = MakeDelegateCommand(() => IsDownloaded = !IsDownloaded);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIStream);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
