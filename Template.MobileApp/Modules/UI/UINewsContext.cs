namespace Template.MobileApp.Modules.UI;

using System.Security.Cryptography;

public sealed partial class UINewsItem : ObservableObject
{
    public required NewsArticle Article { get; init; }

    // 1 時間以内の記事
    public required bool IsNew { get; init; }

    [ObservableProperty]
    public partial bool IsRead { get; set; }
}

// カテゴリごとのページと、保存した記事のページ (Category が null)。記事は初めて表示したときに読み込む
public sealed partial class UINewsPage : ObservableObject
{
    public required NewsCategory? Category { get; init; }

    public bool IsSaved => Category is null;

    // 先頭の大きな記事
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

    // 引っ張って更新で増えた数 (しばらくして 0 に戻す)
    [ObservableProperty(NotifyAlso = [nameof(HasNewArrivals)])]
    public partial int NewArrivals { get; set; }

    public bool HasNewArrivals => NewArrivals > 0;
}

// 一覧と記事が共有する (一覧を開いている間)
public sealed partial class UINewsContext : ObservableObject
{
    private const int PageSize = 10;

    private const int MoreSize = 8;

    private const int MaxCount = 40;

    private const int RelatedCount = 3;

    private readonly TimeProvider timeProvider;

    private readonly NewsFeed feed = new(static x => RandomNumberGenerator.GetInt32(x));

    public IReadOnlyList<UINewsPage> Pages { get; }

    [ObservableProperty]
    public partial NewsArticle Breaking { get; set; }

    // 記事の画面の記事 (Push の前に設定する)
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
        Breaking = feed.CreateBreaking(timeProvider.GetLocalNow().DateTime);
    }

    // 初めて表示したページの記事を、取得に見立てて少し待ってから入れる
    public async Task LoadAsync(UINewsPage page)
    {
        if (!page.IsLoaded && (page.Category is { } category))
        {
            page.IsLoaded = true;

            await Task.Delay(700).ConfigureAwait(true);

            var now = timeProvider.GetLocalNow().DateTime;
            var articles = feed.Create(category, now, PageSize);
            page.Top = ToItem(articles[0], now);
            page.Items.AddRange(articles.Skip(1).Select(x => ToItem(x, now)));
            page.IsLoading = false;
        }
    }

    // 取得に見立てて少し待ってから、新しい記事をトップにし、今のトップを一覧の先頭へ送る (増えた数は NewArrivals)
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
                page.Items.Insert(0, ToItem(feed.CreateLatest(category, now), now));
            }

            page.Top = ToItem(feed.CreateLatest(category, now), now);
            page.NewArrivals = count;
            Breaking = feed.CreateBreaking(now);
        }

        page.IsRefreshing = false;
    }

    // 一覧の最後の記事より古い記事を足す
    public async Task LoadMoreAsync(UINewsPage page)
    {
        if ((page.Category is { } category) && page.IsLoaded && !page.IsLoading && !page.IsLoadingMore && !page.IsEnd && (page.Items.Count > 0))
        {
            page.IsLoadingMore = true;

            await Task.Delay(700).ConfigureAwait(true);

            var now = timeProvider.GetLocalNow().DateTime;
            page.Items.AddRange(feed.Create(category, page.Items[^1].Article.PublishedAt, MoreSize).Select(x => ToItem(x, now)));
            page.IsEnd = page.Items.Count >= MaxCount;
            page.IsLoadingMore = false;
        }
    }

    public void Open(UINewsItem item) =>
        Show(item.Article, feed.CreateRelated(item.Article, RelatedCount));

    public void OpenBreaking() =>
        Show(Breaking, feed.CreateRelated(Breaking, RelatedCount));

    // 今の記事は関連記事の先頭に回す
    public void ShowRelated(NewsArticle article) =>
        Show(article, Related.Where(x => x.Id != article.Id).Prepend(Article).Take(RelatedCount).ToArray());

    // 保存した記事のページは保存した順 (新しい順)
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

    // 一覧に出ている同じ記事は既読にする
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
