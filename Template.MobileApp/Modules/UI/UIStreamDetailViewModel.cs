namespace Template.MobileApp.Modules.UI;

public sealed partial class UIStreamDetailViewModel : AppViewModelBase
{
    // Property

    [Scope]
    [ObservableProperty]
    public partial UIStreamContext Context { get; set; } = default!;

    public IReadOnlyList<string> Friends { get; } =
    [
        "avatar_person01.jpg",
        "avatar_person02.jpg",
        "avatar_person03.jpg",
        "avatar_person04.jpg",
        "avatar_person05.jpg"
    ];

    [ObservableProperty]
    public partial bool TrailersSelected { get; set; } = true;

    [ObservableProperty]
    public partial bool RelatedSelected { get; set; }

    public ScrollController Scroller { get; } = new();

    // Command

    public IObserveCommand SelectTabCommand { get; }

    public IObserveCommand FavoriteCommand { get; }

    public IObserveCommand DownloadCommand { get; }

    public IObserveCommand RelatedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIStreamDetailViewModel()
    {
        SelectTabCommand = MakeDelegateCommand<string>(x =>
        {
            TrailersSelected = x == "Trailers";
            RelatedSelected = !TrailersSelected;
        });
        FavoriteCommand = MakeDelegateCommand(ToggleFavorite);
        DownloadCommand = MakeDelegateCommand(ToggleDownload);
        RelatedCommand = MakeDelegateCommand<StreamItem>(ShowRelated);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Scroller.ScrollTo(0, 0);
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.PopAsync();

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ToggleFavorite() => Context.Selected.IsFavorite = !Context.Selected.IsFavorite;

    private void ToggleDownload() => Context.Selected.IsDownloaded = !Context.Selected.IsDownloaded;

    private void ShowRelated(StreamItem item)
    {
        Context.Open(item);
        Scroller.ScrollTo(0, 0);
    }
}
