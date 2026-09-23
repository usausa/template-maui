namespace Template.MobileApp.Modules.Control;

public sealed class ControlCarouselViewModel : AppViewModelBase
{
    public ObservableCollection<PhotoItem> Items { get; }

    public IObserveCommand CurrentChangedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public ControlCarouselViewModel()
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

        CurrentChangedCommand = MakeDelegateCommand<PhotoItem>(current =>
        {
            foreach (var item in Items)
            {
                item.IsCurrent = item == current;
            }
        });
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ControlMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
