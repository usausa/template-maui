namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class PhotoItem : ObservableObject
{
    public required string Source { get; init; }

    public required long Size { get; init; }

    public required TimeOnly Time { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

public sealed class PhotoSection
{
    public required string Title { get; init; }

    public required string Place { get; init; }

    public required IReadOnlyList<PhotoItem> Items { get; init; }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed class PhotoAlbum
{
    public required IReadOnlyList<PhotoSection> Sections { get; init; }

    public IEnumerable<PhotoItem> Enumerate() => Sections.SelectMany(static x => x.Items);

    public int SelectedCount => Enumerate().Count(static x => x.IsSelected);

    public long SelectedSize => Enumerate().Where(static x => x.IsSelected).Sum(static x => x.Size);

    public void SelectAll(bool value)
    {
        foreach (var item in Enumerate())
        {
            item.IsSelected = value;
        }
    }

    public static void SelectSection(PhotoSection section)
    {
        var fill = section.Items.Any(static x => !x.IsSelected);
        foreach (var item in section.Items)
        {
            item.IsSelected = fill;
        }
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class PhotoSample
{
    private static readonly string[] Sources =
    [
        "photo01.jpg", "photo02.jpg", "photo03.jpg", "photo04.jpg", "photo05.jpg",
        "photo06.jpg", "photo07.jpg", "photo08.jpg", "photo09.jpg", "photo10.jpg"
    ];

    private static readonly (string Title, string Place, int Count, int Hour)[] Plan =
    [
        ("今日", "名古屋市 ・ 東山動植物園", 11, 14),
        ("昨日", "高蔵寺 ・ 自宅", 7, 19),
        ("10 月 23 日", "岐阜市 ・ 金華山", 9, 10),
        ("10 月 20 日", "名古屋市 ・ 大須", 6, 16),
        ("10 月 18 日", "春日井市 ・ 落合公園", 8, 11)
    ];

    public static PhotoAlbum LoadAlbum()
    {
        var sections = new List<PhotoSection>(Plan.Length);
        var seed = 0;
        foreach (var (title, place, count, hour) in Plan)
        {
            var items = new List<PhotoItem>(count);
            for (var i = 0; i < count; i++)
            {
                items.Add(new PhotoItem
                {
                    Source = Sources[seed % Sources.Length],
                    Size = 1_800_000 + (((seed * 733) % 90) * 65_000),
                    Time = new TimeOnly(hour, (seed * 17) % 60)
                });
                seed++;
            }

            sections.Add(new PhotoSection { Title = title, Place = place, Items = items });
        }

        return new PhotoAlbum { Sections = sections };
    }
}
