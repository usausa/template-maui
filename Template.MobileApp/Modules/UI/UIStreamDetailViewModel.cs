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
    public string Title { get; } = "君の知らない空の果てで";
    public string Match { get; } = "98% マッチ";
    public string Meta { get; } = "2024 · SF · 2h 18m · TV-14";
    public string Synopsis { get; } = "巨大都市の空を守る若きパイロットたちが、暴走した AI に眠りを妨げられた古代兵器の起動を阻むため、夕暮れの摩天楼を駆け抜ける。";
    public string CastLine { get; } = "出演: 森 葵、佐藤 蓮、ミア・チェン · 監督: 田中 K.";

    // 視聴中のフレンド (AvatarGroup が MaxDisplayed を超えた分を「+N」にする)
    public IReadOnlyList<string> Friends { get; } =
    [
        "avatar_person01.jpg",
        "avatar_person02.jpg",
        "avatar_person03.jpg",
        "avatar_person04.jpg",
        "avatar_person05.jpg"
    ];

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
        new() { Image = "stream_clip01.jpg", Title = "本予告", Duration = "予告編 · 2m 14s" },
        new() { Image = "stream_clip02.jpg", Title = "ティザー: 管制室", Duration = "ティザー · 1m 02s" },
        new() { Image = "stream_clip03.jpg", Title = "特別映像: 惑星の夜明け", Duration = "特典 · 4m 30s" }
    ];

    public IReadOnlyList<UIStreamDetailRelated> Related { get; } =
    [
        new() { Image = "poster03.jpg", Title = "屋上の約束", Duration = "1h 38m" },
        new() { Image = "poster04.jpg", Title = "キッチン三人組", Duration = "2h 01m" },
        new() { Image = "poster05.jpg", Title = "山の記憶", Duration = "1h 45m" },
        new() { Image = "poster06.jpg", Title = "浮遊城の魔導士", Duration = "1h 52m" }
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
