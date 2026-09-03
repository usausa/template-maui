namespace Template.MobileApp.Models.Sample;

public sealed class NewsItem
{
    public DateTime PublishedAt { get; set; }

    public string CategoryIcon { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Summary { get; set; } = default!;
}
