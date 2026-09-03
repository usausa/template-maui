namespace Template.MobileApp.Modules.View;

public sealed class ViewCollectionViewModel : AppViewModelBase
{
    private const int MaxGroups = 16;

    private int extraIndex;

    public ObservableCollection<AddressGroup> List { get; } = [];

    public IObserveCommand ToggleCommand { get; }

    public IObserveCommand PhoneCommand { get; }
    public IObserveCommand MailCommand { get; }

    public IObserveCommand LoadMoreCommand { get; }

    public ViewCollectionViewModel(
        IDialog dialog)
    {
        ToggleCommand = MakeDelegateCommand<AddressGroup>(g => g.IsExpanded = !g.IsExpanded);
        LoadMoreCommand = MakeDelegateCommand(LoadMore);

        PhoneCommand = MakeAsyncCommand<AddressRow>(async x =>
        {
            var item = x.Value;
            await dialog.InformationAsync($"{item.Name} {item.PhoneNumber}", "Phone");
        });
        MailCommand = MakeAsyncCommand<AddressRow>(async x =>
        {
            var item = x.Value;
            await dialog.InformationAsync($"{item.Name} {item.MailAddress}", "Mail");
        });
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        List.Add(CreateGroup("あ", ["浅井 長政", "安国寺 恵瓊", "井伊 直政", "石田 三成", "上杉 景勝", "宇喜多 秀家"]));
        List.Add(CreateGroup("か", ["加藤 清正", "黒田 長政", "小早川 秀秋"]));
        List.Add(CreateGroup("さ", ["佐竹 義宣", "島 左近", "島津 義弘"]));
        List.Add(CreateGroup("た", ["滝川 一益", "立花 宗茂", "長曾我部 盛親"]));
        List.Add(CreateGroup("な", ["直江 兼続", "鍋島 勝茂"]));
        List.Add(CreateGroup("は", ["平塚 為広", "本多 忠勝", "細川 忠興"]));
        List.Add(CreateGroup("ま", ["前田 利長", "最上 義光", "毛利 輝元"]));
        List.Add(CreateGroup("や", ["山内 一豊"]));
        return Task.CompletedTask;
    }

    // RemainingItemsThresholdReached で末尾に達する前に追加ページを読み込む
    private void LoadMore()
    {
        if (List.Count >= MaxGroups)
        {
            return;
        }

        extraIndex++;
        List.Add(CreateGroup($"追{extraIndex}", [$"追加 家臣 {extraIndex}-1", $"追加 家臣 {extraIndex}-2", $"追加 家臣 {extraIndex}-3"]));
    }

    private static AddressGroup CreateGroup(string key, IEnumerable<string> names) =>
        new(key, names.Select(static (x, i) => new AddressRow(new AddressItem
            {
                Image = "account.png",
                Name = x,
                PhoneNumber = "090-0000-0000",
                MailAddress = "user@example.com"
            })
            { IsEven = i % 2 == 0 }));

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
