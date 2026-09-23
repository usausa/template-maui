namespace Template.MobileApp.Modules.Sample;

public sealed partial class SampleMap2ViewModel : AppViewModelBase
{
    private const double InitialLatitude = 139.767052;
    private const double InitialLongitude = 35.681167;

    private static readonly MapsuiSpot[] Spots =
    [
        new("皇居", "千代田区千代田", 139.752800, 35.685175),
        new("東京タワー", "港区芝公園", 139.745433, 35.658581),
        new("東京スカイツリー", "墨田区押上", 139.810700, 35.710063),
        new("浅草寺", "台東区浅草", 139.796655, 35.714765)
    ];

    private static readonly (double Lon, double Lat)[] RoutePoints =
    [
        (139.752800, 35.685175),
        (139.745433, 35.658581),
        (139.810700, 35.710063),
        (139.796655, 35.714765)
    ];

    private static readonly (double Lon, double Lat)[] AreaPoints =
    [
        (139.746205, 35.693040),
        (139.756673, 35.689457),
        (139.754146, 35.678895),
        (139.742630, 35.679670)
    ];

    private MapsuiSpotManager SpotManager { get; } = new(Spots);

    private MapsuiShapeManager ShapeManager { get; } = new(RoutePoints, AreaPoints);

    private MapsuiGeoJsonManager GeoJsonManager { get; } = new();

    private MapsuiClusterManager ClusterManager { get; } = new(139.75, 35.68, 240);

    public MapsuiController Controller { get; } = new(InitialLatitude, InitialLongitude, 9);

    [ObservableProperty]
    public partial bool WidgetsEnabled { get; set; }

    [ObservableProperty]
    public partial bool SpotsEnabled { get; set; }

    [ObservableProperty]
    public partial bool ShapesEnabled { get; set; }

    [ObservableProperty]
    public partial bool GeoJsonEnabled { get; set; }

    [ObservableProperty]
    public partial bool ClusterEnabled { get; set; }

    [ObservableProperty]
    public partial bool OverlayEnabled { get; set; }

    public ICommand ZoomInCommand { get; }

    public ICommand ZoomOutCommand { get; }

    public ICommand HomeCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SampleMap2ViewModel()
    {
        ZoomInCommand = MakeDelegateCommand(Controller.ZoomIn);
        ZoomOutCommand = MakeDelegateCommand(Controller.ZoomOut);
        HomeCommand = MakeDelegateCommand(() => Controller.MoveTo(InitialLatitude, InitialLongitude));

        Disposables.Add(SpotManager);
        Disposables.Add(ShapeManager);
        Disposables.Add(GeoJsonManager);
        Disposables.Add(ClusterManager);
        Controller.AddManager(new MapsuiWidgetManager());
        Controller.AddManager(SpotManager);
        Controller.AddManager(ShapeManager);
        Controller.AddManager(GeoJsonManager);
        Controller.AddManager(ClusterManager);

        // ウィジェットは初期表示から有効にする
        WidgetsEnabled = true;
        Controller.SetManagerEnabled(MapsuiWidgetManager.ManagerName, true);

        SubscribeWidgetsEnabled(x => Controller.SetManagerEnabled(MapsuiWidgetManager.ManagerName, x));
        SubscribeSpotsEnabled(x => Controller.SetManagerEnabled(MapsuiSpotManager.ManagerName, x));
        SubscribeShapesEnabled(x => Controller.SetManagerEnabled(MapsuiShapeManager.ManagerName, x));
        SubscribeGeoJsonEnabled(x => Controller.SetManagerEnabled(MapsuiGeoJsonManager.ManagerName, x));
        SubscribeClusterEnabled(x => Controller.SetManagerEnabled(MapsuiClusterManager.ManagerName, x));
        SubscribeOverlayEnabled(x => Controller.SetOverlayRoute(x ? RoutePoints : null));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            using var reader = new StreamReader(await FileSystem.OpenAppPackageFileAsync(Path.Combine("Map", "tokyo.geojson")));
            GeoJsonManager.SetGeoJson(await reader.ReadToEndAsync());
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Controller.ZoomIn();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        Controller.ZoomOut();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        Controller.MoveTo(InitialLatitude, InitialLongitude);
        return Task.CompletedTask;
    }
}
