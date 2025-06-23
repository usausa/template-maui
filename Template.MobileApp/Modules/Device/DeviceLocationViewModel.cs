namespace Template.MobileApp.Modules.Device;

public sealed partial class DeviceLocationViewModel : AppViewModelBase
{
    private readonly ILocationService locationService;

    [ObservableProperty]
    public partial Location? Location { get; set; }

    public DeviceLocationViewModel(
        ILocationService locationService)
    {
        this.locationService = locationService;

        Disposables.Add(locationService.LocationChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => Location = x.Location));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        Location = await locationService.GetLastLocationAsync();

        locationService.Start();
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        locationService.Stop();
        return Task.CompletedTask;
    }
}
