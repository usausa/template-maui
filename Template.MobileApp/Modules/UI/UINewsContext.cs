namespace Template.MobileApp.Modules.UI;

using System.Security.Cryptography;

public sealed partial class UINewsItem : ObservableObject
{
    public required NewsArticle Article { get; init; }

    public required bool IsNew { get; init; }

    [ObservableProperty]
    public partial bool IsRead { get; set; }
}

public sealed partial class UINewsPage : ObservableObject
{
    public required NewsCategory? Category { get; init; }

    public bool IsSaved => Category is null;

    [ObservableProperty]
    public partial UINewsItem? Top { get; set; }

    public ObservableCollection<UINewsItem> Items { get; } = [];

    public bool IsLoaded { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingMore { get; set; }

    [ObservableProperty]
    public partial bool IsEnd { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(HasNewArrivals)])]
    public partial int NewArrivals { get; set; }

    public bool HasNewArrivals => NewArrivals > 0;
}

public sealed partial class UINewsContext : ObservableObject
{
    private const int PageSize = 10;

    private const int MoreSize = 8;

    private const int MaxCount = 40;

    private const int RelatedCount = 3;

    private readonly TimeProvider timeProvider;

    private readonly NewsSample sample = new(static x => RandomNumberGenerator.GetInt32(x));

    public IReadOnlyList<UINewsPage> Pages { get; }

    [ObservableProperty]
    public partial NewsArticle Breaking { get; set; }

    [ObservableProperty]
    public partial NewsArticle Article { get; set; } = default!;

    [ObservableProperty]
    public partial IReadOnlyList<NewsArticle> Related { get; set; } = [];

    [ObservableProperty]
    public partial bool IsSaved { get; set; }

    public UINewsContext(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        Pages = Enum.GetValues<NewsCategory>()
            .Select(static x => new UINewsPage { Category = x, IsLoading = true })
            .Append(new UINewsPage { Category = null, IsLoaded = true })
            .ToArray();
        Breaking = sample.LoadBreaking(timeProvider.GetLocalNow().DateTime);
    }

    public async Task LoadAsync(UINewsPage page)
    {
        if (!page.IsLoaded && (page.Category is { } category))
        {
            page.IsLoaded = true;

            await Task.Delay(700).ConfigureAwait(true);

            var now = timeProvider.GetLocalNow().DateTime;
            var articles = sample.LoadArticles(category, now, PageSize);
            page.Top = ToItem(articles[0], now);
            page.Items.AddRange(articles.Skip(1).Select(x => ToItem(x, now)));
            page.IsLoading = false;
        }
    }

    public async Task RefreshAsync(UINewsPage page)
    {
        await Task.Delay(800).ConfigureAwait(true);

        if (page.Category is { } category)
        {
            var now = timeProvider.GetLocalNow().DateTime;
            var count = 1 + RandomNumberGenerator.GetInt32(3);
            if (page.Top is not null)
            {
                page.Items.Insert(0, page.Top);
            }

            for (var i = 1; i < count; i++)
            {
                page.Items.Insert(0, ToItem(sample.LoadLatest(category, now), now));
            }

            page.Top = ToItem(sample.LoadLatest(category, now), now);
            page.NewArrivals = count;
            Breaking = sample.LoadBreaking(now);
        }

        page.IsRefreshing = false;
    }

    public async Task LoadMoreAsync(UINewsPage page)
    {
        if ((page.Category is { } category) && page.IsLoaded && !page.IsLoading && !page.IsLoadingMore && !page.IsEnd && (page.Items.Count > 0))
        {
            page.IsLoadingMore = true;

            await Task.Delay(700).ConfigureAwait(true);

            var now = timeProvider.GetLocalNow().DateTime;
            page.Items.AddRange(sample.LoadArticles(category, page.Items[^1].Article.PublishedAt, MoreSize).Select(x => ToItem(x, now)));
            page.IsEnd = page.Items.Count >= MaxCount;
            page.IsLoadingMore = false;
        }
    }

    public void Open(UINewsItem item) =>
        Show(item.Article, sample.LoadRelated(item.Article, RelatedCount));

    public void OpenBreaking() =>
        Show(Breaking, sample.LoadRelated(Breaking, RelatedCount));

    public void ShowRelated(NewsArticle article) =>
        Show(article, Related.Where(x => x.Id != article.Id).Prepend(Article).Take(RelatedCount).ToArray());

    public void ToggleSave()
    {
        var saved = Pages[^1].Items;
        if (IsSaved)
        {
            if (saved.FirstOrDefault(x => x.Article.Id == Article.Id) is { } item)
            {
                saved.Remove(item);
            }
        }
        else
        {
            saved.Insert(0, FindItem(Article.Id) ?? new UINewsItem { Article = Article, IsNew = false, IsRead = true });
        }

        IsSaved = !IsSaved;
    }

    private static UINewsItem ToItem(NewsArticle article, DateTime now) =>
        new() { Article = article, IsNew = (now - article.PublishedAt) < TimeSpan.FromHours(1) };

    private IEnumerable<UINewsItem> EnumerateItems() =>
        Pages.SelectMany(static x => x.Top is null ? x.Items : x.Items.Prepend(x.Top));

    private UINewsItem? FindItem(int id) =>
        EnumerateItems().FirstOrDefault(x => x.Article.Id == id);

    private void Show(NewsArticle article, IReadOnlyList<NewsArticle> related)
    {
        Article = article;
        Related = related;
        IsSaved = Pages[^1].Items.Any(x => x.Article.Id == article.Id);
        foreach (var item in EnumerateItems().Where(x => x.Article.Id == article.Id))
        {
            item.IsRead = true;
        }
    }
}
