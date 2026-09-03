namespace Template.MobileApp.Modules.Basic;

public sealed partial class BasicSettingViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string UserName { get; set; } = "usausa";

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LastSearch { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double CacheSize { get; set; } = 2;

    [ObservableProperty]
    public partial double Volume { get; set; } = 60;

    [ObservableProperty]
    public partial bool NotificationEnabled { get; set; } = true;

    [ObservableProperty]
    public partial DateTime BackupDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial TimeSpan BackupTime { get; set; } = new(3, 0, 0);

    [ObservableProperty]
    public partial string Quality { get; set; } = "high";

    [ObservableProperty]
    public partial string Language { get; set; } = "日本語";

    public IReadOnlyList<string> Languages { get; } = ["日本語", "English", "中文", "한국어"];

    public IObserveCommand SearchCommand { get; }

    public BasicSettingViewModel()
    {
        SearchCommand = MakeDelegateCommand(() => LastSearch = SearchText);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
