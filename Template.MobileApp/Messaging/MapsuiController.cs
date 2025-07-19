namespace Template.MobileApp.Messaging;

using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.UI.Maui;

public interface IMapsuiController
{
    void Attach(MapControl view);

    void Detach();
}

public sealed class MapsuiController : IMapsuiController
{
    private readonly double homeLongitude;

    private readonly double homeLatitude;

    private readonly int? initialResolution;

    private MapControl? map;

    public MapsuiController(double homeLongitude, double homeLatitude, int? initialResolution = null)
    {
        this.homeLongitude = homeLongitude;
        this.homeLatitude = homeLatitude;
        this.initialResolution = initialResolution;
    }

    void IMapsuiController.Attach(MapControl view)
    {
        map = view;

#pragma warning disable CA2000
        map.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
#pragma warning restore CA2000

        var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(homeLongitude, homeLatitude).ToMPoint();
        if (initialResolution.HasValue)
        {
            map.Map.Navigator.CenterOnAndZoomTo(sphericalMercatorCoordinate, map.Map.Navigator.Resolutions[initialResolution.Value]);
        }
        else
        {
            map.Map.Navigator.CenterOn(sphericalMercatorCoordinate);
        }
    }

    void IMapsuiController.Detach()
    {
        map = null;
    }

    public void MoveTo(double longitude, double latitude, int? resolution = null)
    {
        if (map is null)
        {
            return;
        }

        var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();

        if (resolution.HasValue)
        {
            map.Map.Navigator.CenterOnAndZoomTo(sphericalMercatorCoordinate, map.Map.Navigator.Resolutions[resolution.Value]);
        }
        else
        {
            map.Map.Navigator.CenterOn(sphericalMercatorCoordinate);
        }
    }

    public void ZoomIn()
    {
        map?.Map.Navigator.ZoomIn();
    }

    public void ZoomOut()
    {
        map?.Map.Navigator.ZoomOut();
    }
}
