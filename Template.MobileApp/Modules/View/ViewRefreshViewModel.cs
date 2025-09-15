namespace Template.MobileApp.Modules.View;

using Fonts;

#pragma warning disable CA5394
public sealed partial class ViewRefreshViewModel : AppViewModelBase
{
    private readonly Random random = new();

    public CollectionController Controller { get; } = new();

    public ObservableCollection<NewsItem> Items { get; } = new();

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    public IObserveCommand RefreshCommand { get; }

    public ViewRefreshViewModel()
    {
        RefreshCommand = MakeAsyncCommand(async () =>
        {
            IsRefreshing = true;

            // Dummy wait
            await Task.Delay(1000).ConfigureAwait(true);

            for (var i = 0; i < random.Next(3); i++)
            {
                Items.Insert(0, NewItem());
            }

            IsRefreshing = false;

            Controller.ScrollRequest(0, position: ScrollToPosition.Start);
        });
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        IsRefreshing = true;

        // Dummy wait
        await Task.Delay(1000).ConfigureAwait(true);

        for (var i = 0; i < 10; i++)
        {
            Items.Insert(0, NewItem());
        }

        IsRefreshing = false;
    }

    private NewsItem NewItem()
    {
        var type = random.Next(5);
        var item = new NewsItem { PublishedAt = DateTime.Now };
        switch (type)
        {
            case 0:
                item.CategoryIcon = MaterialIcons.Trending_up;
                item.Title = "経済成長率が予想を上回る";
                item.Summary = "今年度の経済成長率は予想を大きく上回りました。専門家はその要因について分析しています。";
                break;
            case 1:
                item.CategoryIcon = MaterialIcons.Devices;
                item.Title = "新型スマートフォンが発表";
                item.Summary = "最新のスマートフォンが発表され、注目の新機能が多数搭載されています。";
                break;
            case 2:
                item.CategoryIcon = MaterialIcons.Local_movies;
                item.Title = "話題の新作が公開、観客動員数100万人突破";
                item.Summary = "期待の大作が週末の興行収入で新記録を樹立。監督と主演俳優へのインタビューも注目を集めています。";
                break;
            case 3:
                item.CategoryIcon = MaterialIcons.Emoji_events;
                item.Title = "サッカーワールドカップ開幕";
                item.Summary = "待望のワールドカップが本日開幕し、世界中が注目しています。";
                break;
            case 4:
                item.CategoryIcon = MaterialIcons.Restaurant;
                item.Title = "大手牛丼チェーンが新メニューを発表！期間限定で特製カレーが登場";
                item.Summary = "全国展開する牛丼チェーンが、創業以来初の本格カレーメニューを発売。SNSでも大きな反響を呼んでいます。";
                break;
        }
        return item;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
#pragma warning restore CA5394
