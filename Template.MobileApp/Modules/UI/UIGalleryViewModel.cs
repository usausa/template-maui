namespace Template.MobileApp.Modules.UI;

public sealed partial class UIGalleryViewModel : AppViewModelBase
{
    // Field

    private const int WideColumns = 2;

    private const int NormalColumns = 3;

    private const int DenseColumns = 5;

    private const double WideScale = 1.35;

    private const double DenseScale = 0.75;

    private readonly IShare share;

    // Property

    public PhotoAlbum Album { get; } = PhotoSample.LoadAlbum();

    [ObservableProperty]
    public partial int Columns { get; set; } = NormalColumns;

    [ObservableProperty]
    public partial double ZoomScale { get; set; } = 1;

    [ObservableProperty]
    public partial int SelectedCount { get; set; }

    [ObservableProperty]
    public partial long SelectedSize { get; set; }

    [ObservableProperty]
    public partial PhotoSection StickySection { get; set; }

    // Command

    public IObserveCommand TapCommand { get; }

    public IObserveCommand SectionCommand { get; }

    public IObserveCommand ColumnCommand { get; }

    public IObserveCommand ScrolledCommand { get; }

    public IObserveCommand ShareCommand { get; }

    public IObserveCommand ClearCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIGalleryViewModel(IShare share)
    {
        this.share = share;

        StickySection = Album.Sections[0];

        SubscribeZoomScale(UpdateColumns);

        TapCommand = MakeDelegateCommand<PhotoItem>(CommandMode.Simple, Toggle);
        SectionCommand = MakeDelegateCommand<PhotoSection>(CommandMode.Simple, x =>
        {
            PhotoAlbum.SelectSection(x);
            UpdateSelection();
        });
        ColumnCommand = MakeDelegateCommand<int>(CommandMode.Simple, x =>
        {
            Columns = x;
            ZoomScale = x == WideColumns ? WideScale : x == DenseColumns ? DenseScale : 1;
        });
        ScrolledCommand = MakeDelegateCommand<int>(CommandMode.Simple, x => StickySection = Album.Sections[x]);
        ShareCommand = MakeAsyncCommand(ShareAsync);
        ClearCommand = MakeDelegateCommand(() =>
        {
            Album.SelectAll(false);
            UpdateSelection();
        });

        UpdateSelection();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Toggle(PhotoItem item)
    {
        item.IsSelected = !item.IsSelected;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        SelectedCount = Album.SelectedCount;
        SelectedSize = Album.SelectedSize;
    }

    private void UpdateColumns(double value) =>
        Columns = value >= WideScale ? WideColumns : value <= DenseScale ? DenseColumns : NormalColumns;

    private Task ShareAsync() =>
        share.RequestAsync(new ShareTextRequest
        {
            Title = "写真を共有",
            Text = $"{SelectedCount} 件の写真を共有します"
        });
}
