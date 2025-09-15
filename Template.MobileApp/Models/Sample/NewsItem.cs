namespace Template.MobileApp.Models.Sample;

#pragma warning disable CA1056
public sealed class NewsItem
{
    public DateTime PublishedAt { get; set; }

    public string CategoryIcon { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Summary { get; set; } = default!;
}
#pragma warning restore CA1056
