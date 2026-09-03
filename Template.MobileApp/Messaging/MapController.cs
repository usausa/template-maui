namespace Template.MobileApp.Messaging;

using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

public interface IMapController
{
    void Attach(Map view);

    void Detach();
}

public sealed class MapController : IMapController
{
    private readonly Location homeLocation;

    private readonly Distance homeRadius;

    private Map? map;

    public MapController(double latitude, double longitude, double radiusKilometers)
    {
        homeLocation = new Location(latitude, longitude);
        homeRadius = Distance.FromKilometers(radiusKilometers);
    }

    void IMapController.Attach(Map view)
    {
        map = view;
        map.MoveToRegion(MapSpan.FromCenterAndRadius(homeLocation, homeRadius));
    }

    void IMapController.Detach()
    {
        map = null;
    }

    public void MoveToHome()
    {
        map?.MoveToRegion(MapSpan.FromCenterAndRadius(homeLocation, homeRadius));
    }

    public void MoveTo(double latitude, double longitude, double radiusKilometers = 1d)
    {
        map?.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(latitude, longitude), Distance.FromKilometers(radiusKilometers)));
    }

    //--------------------------------------------------------------------------------
    // MapElements (Polyline / Polygon / Circle)
    //--------------------------------------------------------------------------------

    private Polyline? routeElement;

    private Polygon? areaElement;

    private Circle? circleElement;

    // 経路 (null で消去)
    public void SetRoute(IEnumerable<Location>? points)
    {
        if (map is null)
        {
            return;
        }

        if (routeElement is not null)
        {
            map.MapElements.Remove(routeElement);
            routeElement = null;
        }

        if (points is not null)
        {
            var element = new Polyline
            {
                StrokeColor = Color.FromArgb("#1E88E5"),
                StrokeWidth = 8
            };
            foreach (var point in points)
            {
                element.Geopath.Add(point);
            }

            map.MapElements.Add(element);
            routeElement = element;
        }
    }

    // 範囲 (null で消去。先頭と末尾は自動で接続される)
    public void SetArea(IEnumerable<Location>? points)
    {
        if (map is null)
        {
            return;
        }

        if (areaElement is not null)
        {
            map.MapElements.Remove(areaElement);
            areaElement = null;
        }

        if (points is not null)
        {
            var element = new Polygon
            {
                StrokeColor = Color.FromArgb("#43A047"),
                StrokeWidth = 4,
                FillColor = Color.FromArgb("#3343A047")
            };
            foreach (var point in points)
            {
                element.Geopath.Add(point);
            }

            map.MapElements.Add(element);
            areaElement = element;
        }
    }

    // 半径円 (null で消去)
    public void SetCircle(Location? center, double radiusKilometers = 1d)
    {
        if (map is null)
        {
            return;
        }

        if (circleElement is not null)
        {
            map.MapElements.Remove(circleElement);
            circleElement = null;
        }

        if (center is not null)
        {
            var element = new Circle
            {
                Center = center,
                Radius = Distance.FromKilometers(radiusKilometers),
                StrokeColor = Color.FromArgb("#E53935"),
                StrokeWidth = 4,
                FillColor = Color.FromArgb("#22E53935")
            };
            map.MapElements.Add(element);
            circleElement = element;
        }
    }
}
