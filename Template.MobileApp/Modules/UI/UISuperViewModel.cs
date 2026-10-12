namespace Template.MobileApp.Modules.UI;

public sealed partial class UISuperViewModel : AppViewModelBase
{
    private readonly IDispatcherTimer timer;

    [ObservableProperty]
    public partial int BannerPosition { get; set; }

    public int Point { get; } = SuperSample.LoadPoint();

    public IReadOnlyList<SuperBanner> Banners { get; } = SuperSample.LoadBanners();

    public IReadOnlyList<SuperApp> Apps { get; } = SuperSample.LoadApps();

    public IReadOnlyList<SuperCoupon> Coupons { get; } = SuperSample.LoadCoupons();

    public IReadOnlyList<SuperShop> Shops { get; } = SuperSample.LoadShops();

    public IObserveCommand AcquireCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UISuperViewModel(IDispatcher dispatcher)
    {
        // バナーは 5 秒毎に自動スライドする
        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(5);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => BannerPosition = (BannerPosition + 1) % Banners.Count));

        AcquireCommand = MakeDelegateCommand<SuperCoupon>(x => x.IsAcquired = !x.IsAcquired);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        timer.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
