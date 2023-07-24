namespace Template.MobileApp.Modules.Device;

public class DeviceLocationViewModel : AppViewModelBase
{
    private readonly ILocationService locationService;

    public NotificationValue<Location?> Location { get; } = new();

    public DeviceLocationViewModel(
        ApplicationState applicationState,
        ILocationService locationService)
        : base(applicationState)
    {
        this.locationService = locationService;

        Disposables.Add(Observable
            .FromEvent<EventHandler<LocationEventArgs>, LocationEventArgs>(static h => (_, e) => h(e), h => locationService.LocationChanged += h, h => locationService.LocationChanged -= h)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(x => Location.Value = x.Location));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    public override async void OnNavigatedTo(INavigationContext context)
    {
        Location.Value = await locationService.GetLastLocationAsync();

        locationService.Start(GeolocationAccuracy.Best);
    }

    public override void OnNavigatingFrom(INavigationContext context)
    {
        locationService.Stop();
    }
}
