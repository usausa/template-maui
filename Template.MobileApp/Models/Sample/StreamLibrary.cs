namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum StreamGenre
{
    Sf,
    Action,
    Romance,
    Comedy,
    Documentary,
    Fantasy
}

public enum StreamBadge
{
    None,
    New,
    NewSeason,
    Live
}

public enum StreamTrailerKind
{
    Trailer,
    Teaser,
    Extra
}

// 棚 (高評価・オリジナル・急上昇・アクション & アドベンチャー)
public enum StreamShelf
{
    TopRated,
    Original,
    Trending,
    Action
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record StreamTrailer(string Image, string Title, StreamTrailerKind Kind, TimeSpan Length);

// Minutes は上映時間 (分)、Match は好みとの一致 (%)
public sealed record StreamWork(
    int Id,
    string Title,
    string Image,
    int Year,
    StreamGenre Genre,
    int Minutes,
    string Rated,
    double Rating,
    int Match,
    StreamBadge Badge,
    bool IsFeatured,
    bool IsOriginal,
    string Synopsis,
    string Cast,
    string Director,
    IReadOnlyList<StreamTrailer> Trailers);

// 作品と見る人の状態
public sealed partial class StreamItem : ObservableObject
{
    public required StreamWork Work { get; init; }

    // 見た割合 (0 は未視聴)
    public double Progress { get; init; }

    public bool HasProgress => Progress > 0;

    public int RemainMinutes => (int)Math.Ceiling(Work.Minutes * (1 - Progress));

    [ObservableProperty]
    public partial bool InMyList { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial bool IsDownloaded { get; set; }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

// 見る人の作品の一覧 (トップ・続きを見る・棚・関連)
public sealed class StreamLibrary
{
    private const int RelatedCount = 4;

    public IReadOnlyList<StreamItem> Items { get; }

    public StreamLibrary(IReadOnlyList<StreamItem> items)
    {
        Items = items;
    }

    public IReadOnlyList<StreamItem> Featured() => Items.Where(static x => x.Work.IsFeatured).ToArray();

    public IReadOnlyList<StreamItem> Continue() => Items.Where(static x => x.HasProgress).ToArray();

    public IReadOnlyList<StreamItem> Shelf(StreamShelf shelf) => shelf switch
    {
        StreamShelf.TopRated => Items.OrderByDescending(static x => x.Work.Rating).ToArray(),
        StreamShelf.Original => Items.Where(static x => x.Work.IsOriginal).ToArray(),
        StreamShelf.Trending => Items.OrderByDescending(static x => x.Work.Year).ThenByDescending(static x => x.Work.Match).ToArray(),
        _ => Items.Where(static x => x.Work.Genre is StreamGenre.Action or StreamGenre.Sf or StreamGenre.Fantasy).ToArray()
    };

    // 関連は同じジャンルの作品を先に
    public IReadOnlyList<StreamItem> Related(StreamItem item) =>
        Items
            .Where(x => x != item)
            .OrderBy(x => x.Work.Genre == item.Work.Genre ? 0 : 1)
            .Take(RelatedCount)
            .ToArray();
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// 作品の見本 (見ている途中の作品は見た割合も)
public static class StreamSample
{
    private static readonly IReadOnlyList<StreamWork> Works =
    [
        new(
            1, "君の知らない空の果てで", "stream_hero.jpg", 2024, StreamGenre.Sf, 138, "TV-14", 8.4, 98, StreamBadge.NewSeason, true, true,
            "巨大都市の空を守る若きパイロットたちが、暴走した AI に眠りを妨げられた古代兵器の起動を阻むため、夕暮れの摩天楼を駆け抜ける。",
            "森 葵、佐藤 蓮、ミア・チェン", "田中 K.",
            [
                new("stream_clip01.jpg", "本予告", StreamTrailerKind.Trailer, new TimeSpan(0, 2, 14)),
                new("stream_clip02.jpg", "ティザー: 管制室", StreamTrailerKind.Teaser, new TimeSpan(0, 1, 2)),
                new("stream_clip03.jpg", "特別映像: 惑星の夜明け", StreamTrailerKind.Extra, new TimeSpan(0, 4, 30))
            ]),
        new(
            2, "星海のリング", "poster01.jpg", 2025, StreamGenre.Sf, 102, "TV-PG", 8.1, 95, StreamBadge.New, true, true,
            "銀河の果ての巨大なリングに取り残された宇宙飛行士が、仲間の待つ地球へ帰るため、最後の航路を探す。",
            "高橋 湊、小林 美月", "中村 S.",
            [new("poster01.jpg", "本予告", StreamTrailerKind.Trailer, new TimeSpan(0, 1, 48))]),
        new(
            3, "紅の残響", "poster02.jpg", 2023, StreamGenre.Action, 125, "TV-MA", 7.9, 91, StreamBadge.None, true, false,
            "雨の降る裏通りで、赤い刀を持つ剣士が、姉を奪った組織への復讐のため一夜の戦いに挑む。",
            "黒川 玲、伊藤 剛", "山本 R.",
            [new("poster02.jpg", "本予告", StreamTrailerKind.Trailer, new TimeSpan(0, 2, 5))]),
        new(
            4, "屋上の約束", "poster03.jpg", 2024, StreamGenre.Romance, 98, "TV-G", 8.6, 93, StreamBadge.Live, true, true,
            "卒業の前の日に屋上で交わした約束。十年後の同じ日、二人は夕焼けの屋上で再会する。",
            "春野 陽太、水瀬 さくら", "加藤 M.",
            [new("poster03.jpg", "本予告", StreamTrailerKind.Trailer, new TimeSpan(0, 1, 32))]),
        new(
            5, "キッチン三人組", "poster04.jpg", 2025, StreamGenre.Comedy, 58, "TV-G", 7.6, 88, StreamBadge.None, false, true,
            "料理の苦手な三人が、つぶれかけの洋菓子店を立て直すため、毎日の試作に挑む。笑いと甘さの日々。",
            "桃井 ひな、黄瀬 りん、栗原 まゆ", "吉田 T.",
            [new("poster04.jpg", "本予告", StreamTrailerKind.Trailer, new TimeSpan(0, 1, 15))]),
        new(
            6, "山の記憶", "poster05.jpg", 2022, StreamGenre.Documentary, 72, "TV-G", 8.2, 86, StreamBadge.None, false, false,
            "山岳写真家が、亡き父の残した地図を頼りに、北アルプスの尾根をたどる記録。",
            "大石 誠", "森田 H.",
            [new("poster05.jpg", "本予告", StreamTrailerKind.Trailer, new TimeSpan(0, 2, 20))]),
        new(
            7, "浮遊城の魔導士", "poster06.jpg", 2025, StreamGenre.Fantasy, 112, "TV-PG", 8.3, 94, StreamBadge.None, true, true,
            "空に浮かぶ城を守る若き魔導士が、城を落とそうとする古い竜との契約の秘密に迫る。",
            "翠川 レイ、白石 カイ", "西村 A.",
            [new("poster06.jpg", "本予告", StreamTrailerKind.Trailer, new TimeSpan(0, 1, 56))])
    ];

    public static IReadOnlyList<StreamItem> LoadItems() =>
        [.. Works.Select(static x => new StreamItem { Work = x, Progress = ProgressOf(x.Id) })];

    // 見本の視聴の進み具合
    private static double ProgressOf(int id) => id switch
    {
        1 => 0.4,
        5 => 0.7,
        6 => 0.25,
        _ => 0
    };
}
