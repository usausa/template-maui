namespace Template.MobileApp.Modules.Sample;

using Microsoft.Maui.Maps;

public sealed partial class SampleMap1ViewModel : AppViewModelBase
{
    // Field

    private const double InitialLatitude = 35.681167;
    private const double InitialLongitude = 139.767052;
    private static readonly Location[] AreaPoints =
    [
        new(35.693040, 139.746205),
        new(35.689457, 139.756673),
        new(35.678895, 139.754146),
        new(35.679670, 139.742630),
        new(35.687168, 139.740683)
    ];

    // Map

    public MapController Controller { get; } = new(InitialLatitude, InitialLongitude, 3);

    [ObservableProperty]
    public partial MapType CurrentMapType { get; set; } = MapType.Street;

    public IReadOnlyList<MapSpot> Spots { get; } = MapSample.LoadSpots();

    // Layer

    [ObservableProperty]
    public partial bool HybridEnabled { get; set; }

    [ObservableProperty]
    public partial bool RouteVisible { get; set; }

    [ObservableProperty]
    public partial bool AreaVisible { get; set; }

    [ObservableProperty]
    public partial bool CircleVisible { get; set; }

    [ObservableProperty]
    public partial bool IsLayerOpen { get; set; }

    // Command

    public ICommand HomeCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SampleMap1ViewModel()
    {
        HomeCommand = MakeDelegateCommand(Controller.MoveToHome);

        SubscribeHybridEnabled(x => CurrentMapType = x ? MapType.Hybrid : MapType.Street);
        SubscribeRouteVisible(x => Controller.SetRoute(x ? Spots.Select(static spot => spot.Location) : null));
        SubscribeAreaVisible(x => Controller.SetArea(x ? AreaPoints : null));
        SubscribeCircleVisible(x => Controller.SetCircle(x ? new Location(InitialLatitude, InitialLongitude) : null, 1.5));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync()
    {
        if (IsLayerOpen)
        {
            IsLayerOpen = false;
            return Task.CompletedTask;
        }

        return Navigator.ForwardAsync(ViewId.SampleMenu);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        IsLayerOpen = !IsLayerOpen;
        return Task.CompletedTask;
    }
}
