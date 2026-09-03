namespace Template.MobileApp.Modules.View;

public sealed class ViewCarouselViewModel : AppViewModelBase
{
    public ObservableCollection<PhotoItem> Items { get; }

    public IObserveCommand CurrentChangedCommand { get; }

    public ViewCarouselViewModel()
    {
        Items =
        [
            new() { Title = "風景1", Description = "景色の画像1", ImageUrl = "https://picsum.photos/500/500?random=1" },
            new() { Title = "風景2", Description = "景色の画像2", ImageUrl = "https://picsum.photos/500/500?random=2" },
            new() { Title = "風景3", Description = "景色の画像3", ImageUrl = "https://picsum.photos/500/500?random=3" },
            new() { Title = "風景4", Description = "景色の画像4", ImageUrl = "https://picsum.photos/500/500?random=4" },
            new() { Title = "風景5", Description = "景色の画像5", ImageUrl = "https://picsum.photos/500/500?random=5" }
        ];
        Items[0].IsCurrent = true;

        // 中央のカードのみ強調(両脇は縮小・淡色化)するため現在項目をマークする
        CurrentChangedCommand = MakeDelegateCommand<PhotoItem>(current =>
        {
            foreach (var item in Items)
            {
                item.IsCurrent = item == current;
            }
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
