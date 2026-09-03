namespace Template.MobileApp.Modules.Sample;

using Microsoft.Maui.Maps;

public sealed partial class SampleMap1ViewModel : AppViewModelBase
{
    // 東京駅
    private const double InitialLatitude = 35.681167;
    private const double InitialLongitude = 139.767052;

    public MapController Controller { get; } = new(InitialLatitude, InitialLongitude, 3);

    [ObservableProperty]
    public partial MapType CurrentMapType { get; set; } = MapType.Street;

    public IReadOnlyList<MapSpot> Spots { get; } =
    [
        new() { Name = "皇居", Description = "千代田区千代田", Location = new Location(35.685175, 139.752800) },
        new() { Name = "東京タワー", Description = "港区芝公園", Location = new Location(35.658581, 139.745433) },
        new() { Name = "東京スカイツリー", Description = "墨田区押上", Location = new Location(35.710063, 139.810700) },
        new() { Name = "浅草寺", Description = "台東区浅草", Location = new Location(35.714765, 139.796655) }
    ];

    // 皇居周辺の範囲 (Polygon)
    private static readonly Location[] AreaPoints =
    [
        new(35.693040, 139.746205),
        new(35.689457, 139.756673),
        new(35.678895, 139.754146),
        new(35.679670, 139.742630),
        new(35.687168, 139.740683)
    ];

    [ObservableProperty]
    public partial bool RouteVisible { get; set; }

    [ObservableProperty]
    public partial bool AreaVisible { get; set; }

    [ObservableProperty]
    public partial bool CircleVisible { get; set; }

    public ICommand HomeCommand { get; }

    public ICommand ToggleMapTypeCommand { get; }

    public ICommand ToggleRouteCommand { get; }

    public ICommand ToggleAreaCommand { get; }

    public ICommand ToggleCircleCommand { get; }

    public SampleMap1ViewModel()
    {
        HomeCommand = MakeDelegateCommand(Controller.MoveToHome);
        ToggleMapTypeCommand = MakeDelegateCommand(() => CurrentMapType = CurrentMapType == MapType.Street ? MapType.Hybrid : MapType.Street);

        // MapElements のデモ: 経路 (スポット巡回) / 範囲 (皇居周辺) / 半径円 (東京駅 1.5km)
        ToggleRouteCommand = MakeDelegateCommand(() =>
        {
            RouteVisible = !RouteVisible;
            Controller.SetRoute(RouteVisible ? Spots.Select(static x => x.Location) : null);
        });
        ToggleAreaCommand = MakeDelegateCommand(() =>
        {
            AreaVisible = !AreaVisible;
            Controller.SetArea(AreaVisible ? AreaPoints : null);
        });
        ToggleCircleCommand = MakeDelegateCommand(() =>
        {
            CircleVisible = !CircleVisible;
            Controller.SetCircle(CircleVisible ? new Location(InitialLatitude, InitialLongitude) : null, 1.5);
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
