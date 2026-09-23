namespace Template.MobileApp.Modules.Device;

public sealed partial class DeviceLocationViewModel : AppViewModelBase
{
    private readonly ILocationService locationService;

    private readonly IGeolocation geolocation;

    [ObservableProperty]
    public partial Location? Location { get; set; }

    // 端末の位置情報サービスが無効 (測位待ちの空状態に理由を出す)
    [ObservableProperty]
    public partial bool ServiceDisabled { get; set; }

    // 初期表示は東京駅周辺(測位後に現在地へ移動)
    public MapController Controller { get; } = new(35.681236, 139.767125, 3);

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DeviceLocationViewModel(
        ILocationService locationService,
        IGeolocation geolocation)
    {
        this.locationService = locationService;
        this.geolocation = geolocation;

        Disposables.Add(locationService.LocationChangedAsObservable().ObserveOnCurrentContext().Subscribe(x =>
        {
            Location = x.Location;
            Controller.MoveTo(x.Location.Latitude, x.Location.Longitude);
        }));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (await Permissions.RequestLocationAsync())
        {
            ServiceDisabled = !geolocation.IsEnabled;

            Location = await locationService.GetLastLocationAsync();
            if (Location is not null)
            {
                Controller.MoveTo(Location.Latitude, Location.Longitude);
            }

            locationService.Start();
        }
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        locationService.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
