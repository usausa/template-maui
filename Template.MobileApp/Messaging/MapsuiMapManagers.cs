namespace Template.MobileApp.Messaging;

using System.Text.Json;

using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.ScaleBar;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

using MapsuiBrush = Mapsui.Styles.Brush;
using MapsuiColor = Mapsui.Styles.Color;
using MapsuiMap = Mapsui.Map;
using MapsuiPen = Mapsui.Styles.Pen;

// Mapsui の機能を「機能グループ別マネージャ」に分割し、コントローラの辞書 (Mapper) に登録して
// 個別に有効/無効化する。MPowerKit/GoogleMaps のマネージャ分割設計を参考にした構成
public interface IMapsuiMapManager
{
    string Name { get; }

    void Attach(MapsuiMap map);

    void Detach(MapsuiMap map);
}

public sealed record MapsuiSpot(string Name, string Subtitle, double Longitude, double Latitude);

//--------------------------------------------------------------------------------
// Widget (ScaleBar / ZoomInOut)
//--------------------------------------------------------------------------------

public sealed class MapsuiWidgetManager : IMapsuiMapManager
{
    public const string ManagerName = "Widget";

    private ScaleBarWidget? scaleBar;

    private ZoomInOutWidget? zoomInOut;

    public string Name => ManagerName;

    public void Attach(MapsuiMap map)
    {
        // Map.Widgets はキューのため取り外しできない。生成済みなら Enabled を戻すだけにする
        if (scaleBar is null)
        {
            scaleBar = new ScaleBarWidget(map)
            {
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom,
                Margin = new Mapsui.MRect(16),
                TextAlignment = Mapsui.Widgets.Alignment.Center
            };
            zoomInOut = new ZoomInOutWidget
            {
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top,
                Margin = new Mapsui.MRect(16)
            };
            map.Widgets.Enqueue(scaleBar);
            map.Widgets.Enqueue(zoomInOut);
        }
        else
        {
            scaleBar.Enabled = true;
            zoomInOut!.Enabled = true;
        }

        map.RefreshGraphics();
    }

    public void Detach(MapsuiMap map)
    {
        if (scaleBar is not null)
        {
            scaleBar.Enabled = false;
            zoomInOut!.Enabled = false;
            map.RefreshGraphics();
        }
    }
}

//--------------------------------------------------------------------------------
// Spot (Pin + Callout)
//--------------------------------------------------------------------------------

public sealed class MapsuiSpotManager : IMapsuiMapManager, IDisposable
{
    public const string ManagerName = "Spot";

    private readonly MemoryLayer layer;

    private readonly List<PointFeature> features = [];

    public string Name => ManagerName;

    public void Dispose() => layer.Dispose();

    public MapsuiSpotManager(IEnumerable<MapsuiSpot> spots)
    {
        foreach (var spot in spots)
        {
            var point = SphericalMercator.FromLonLat(spot.Longitude, spot.Latitude).ToMPoint();
            var feature = new PointFeature(point);
            feature.Styles.Add(new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                SymbolScale = 0.6,
                Fill = new MapsuiBrush(MapsuiColor.FromString("#E53935")),
                Outline = new MapsuiPen(MapsuiColor.White, 2)
            });
            feature.Styles.Add(new CalloutStyle
            {
                Title = spot.Name,
                Subtitle = spot.Subtitle,
                TitleFontColor = MapsuiColor.Black,
                SubtitleFontColor = MapsuiColor.FromString("#757575"),
                Type = CalloutType.Detail,
                Offset = new Offset(0, -12),
                Enabled = false
            });
            features.Add(feature);
        }

        layer = new MemoryLayer(ManagerName) { Features = features };
    }

    public void Attach(MapsuiMap map)
    {
        map.Layers.Add(layer);
        map.Tapped += OnMapTapped;
    }

    public void Detach(MapsuiMap map)
    {
        map.Tapped -= OnMapTapped;
        map.Layers.Remove(layer);
    }

    // タップしたピンのコールアウトをトグルする (他のピンは閉じる)
    private void OnMapTapped(object? sender, Mapsui.MapEventArgs e)
    {
        var info = e.GetMapInfo([layer]);
        var tapped = info.Feature;

        foreach (var feature in features)
        {
            foreach (var style in feature.Styles)
            {
                if (style is CalloutStyle callout)
                {
                    callout.Enabled = ReferenceEquals(feature, tapped) && !callout.Enabled;
                }
            }
        }

        layer.DataHasChanged();

        if (tapped is not null)
        {
            e.Handled = true;
        }
    }
}

//--------------------------------------------------------------------------------
// Shape (Polyline / Polygon)
//--------------------------------------------------------------------------------

public sealed class MapsuiShapeManager : IMapsuiMapManager, IDisposable
{
    public const string ManagerName = "Shape";

    private readonly MemoryLayer layer;

    public string Name => ManagerName;

    public void Dispose() => layer.Dispose();

    public MapsuiShapeManager(IReadOnlyList<(double Lon, double Lat)> route, IReadOnlyList<(double Lon, double Lat)> area)
    {
        var routeFeature = new GeometryFeature(new LineString(ToMercator(route)));
        routeFeature.Styles.Add(new VectorStyle
        {
            Line = new MapsuiPen(MapsuiColor.FromString("#1E88E5"), 4)
        });

        var ring = new List<(double Lon, double Lat)>(area) { area[0] };
        var areaFeature = new GeometryFeature(new Polygon(new LinearRing(ToMercator(ring))));
        areaFeature.Styles.Add(new VectorStyle
        {
            Fill = new MapsuiBrush(new MapsuiColor(0x43, 0xA0, 0x47, 0x33)),
            Outline = new MapsuiPen(MapsuiColor.FromString("#43A047"), 2)
        });

        layer = new MemoryLayer(ManagerName) { Features = new List<Mapsui.IFeature> { routeFeature, areaFeature } };
    }

    public void Attach(MapsuiMap map) => map.Layers.Add(layer);

    public void Detach(MapsuiMap map) => map.Layers.Remove(layer);

    private static Coordinate[] ToMercator(IReadOnlyList<(double Lon, double Lat)> points)
    {
        var coordinates = new Coordinate[points.Count];
        for (var i = 0; i < points.Count; i++)
        {
            var (x, y) = SphericalMercator.FromLonLat(points[i].Lon, points[i].Lat);
            coordinates[i] = new Coordinate(x, y);
        }

        return coordinates;
    }
}

//--------------------------------------------------------------------------------
// GeoJSON
//--------------------------------------------------------------------------------

public sealed class MapsuiGeoJsonManager : IMapsuiMapManager, IDisposable
{
    public const string ManagerName = "GeoJson";

    private readonly MemoryLayer layer = new(ManagerName);

    private MapsuiMap? attachedMap;

    public string Name => ManagerName;

    public void Dispose() => layer.Dispose();

    public void Attach(MapsuiMap map)
    {
        attachedMap = map;
        map.Layers.Add(layer);
    }

    public void Detach(MapsuiMap map)
    {
        map.Layers.Remove(layer);
        attachedMap = null;
    }

    private static readonly JsonSerializerOptions GeoJsonOptions = CreateGeoJsonOptions();

    private static JsonSerializerOptions CreateGeoJsonOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new GeoJsonConverterFactory());
        return options;
    }

    // GeoJSON (EPSG:4326) を NetTopologySuite で読み、球面メルカトルへ再投影して表示する
    public void SetGeoJson(string json)
    {
        var collection = JsonSerializer.Deserialize<NetTopologySuite.Features.FeatureCollection>(json, GeoJsonOptions);
        if (collection is null)
        {
            return;
        }

        var features = new List<Mapsui.IFeature>();
        foreach (var source in collection)
        {
            var geometry = source.Geometry;
            geometry.Apply(ToMercatorFilter.Instance);

            var feature = new GeometryFeature(geometry);
            var name = source.Attributes?.GetOptionalValue("name")?.ToString() ?? string.Empty;
            switch (geometry)
            {
                case LineString:
                    feature.Styles.Add(new VectorStyle
                    {
                        Line = new MapsuiPen(MapsuiColor.FromString("#FB8C00"), 4)
                    });
                    break;
                case Polygon or MultiPolygon:
                    feature.Styles.Add(new VectorStyle
                    {
                        Fill = new MapsuiBrush(new MapsuiColor(0x8E, 0x24, 0xAA, 0x33)),
                        Outline = new MapsuiPen(MapsuiColor.FromString("#8E24AA"), 2)
                    });
                    break;
                default:
                    feature.Styles.Add(new SymbolStyle
                    {
                        SymbolType = SymbolType.Triangle,
                        SymbolScale = 0.6,
                        Fill = new MapsuiBrush(MapsuiColor.FromString("#8E24AA")),
                        Outline = new MapsuiPen(MapsuiColor.White, 2)
                    });
                    break;
            }

            if (name.Length > 0)
            {
                feature.Styles.Add(new LabelStyle
                {
                    Text = name,
                    ForeColor = MapsuiColor.FromString("#4A148C"),
                    BackColor = new MapsuiBrush(new MapsuiColor(0xFF, 0xFF, 0xFF, 0xCC)),
                    Offset = new Offset(0, -24)
                });
            }

            features.Add(feature);
        }

        layer.Features = features;
        layer.DataHasChanged();
        attachedMap?.RefreshData();
    }

    // 座標を EPSG:4326 → EPSG:3857 に書き換えるフィルタ
    private sealed class ToMercatorFilter : ICoordinateFilter
    {
        public static readonly ToMercatorFilter Instance = new();

        public void Filter(Coordinate coord)
        {
            var (x, y) = SphericalMercator.FromLonLat(coord.X, coord.Y);
            coord.X = x;
            coord.Y = y;
        }
    }
}

//--------------------------------------------------------------------------------
// Cluster (ズームレベルに応じたグリッドクラスタリング)
//--------------------------------------------------------------------------------

#pragma warning disable CA5394
public sealed class MapsuiClusterManager : IMapsuiMapManager, IDisposable
{
    public const string ManagerName = "Cluster";

    private const double CellPixels = 72d;

    private readonly MemoryLayer layer = new(ManagerName);

    private readonly List<Mapsui.MPoint> points = [];

    private MapsuiMap? attachedMap;

    private double lastResolution = -1d;

    public string Name => ManagerName;

    public void Dispose() => layer.Dispose();

    public MapsuiClusterManager(double centerLongitude, double centerLatitude, int count)
    {
        // 疑似乱数 (固定シード) で中心周辺に点群を作る
        var random = new Random(42);
        for (var i = 0; i < count; i++)
        {
            var lon = centerLongitude + ((random.NextDouble() - 0.5) * 0.7);
            var lat = centerLatitude + ((random.NextDouble() - 0.5) * 0.5);
            points.Add(SphericalMercator.FromLonLat(lon, lat).ToMPoint());
        }
    }

    public void Attach(MapsuiMap map)
    {
        attachedMap = map;
        map.Layers.Add(layer);
        map.Navigator.ViewportChanged += OnViewportChanged;
        lastResolution = -1d;
        Rebuild();
    }

    public void Detach(MapsuiMap map)
    {
        map.Navigator.ViewportChanged -= OnViewportChanged;
        map.Layers.Remove(layer);
        attachedMap = null;
    }

    private void OnViewportChanged(object? sender, EventArgs e) => Rebuild();

    // 解像度 (ズーム) が変わったときだけ、画面上おおよそ CellPixels 四方のグリッドへまとめ直す
    private void Rebuild()
    {
        var map = attachedMap;
        if (map is null)
        {
            return;
        }

        var resolution = map.Navigator.Viewport.Resolution;
        if ((resolution <= 0d) || (Math.Abs(resolution - lastResolution) < (lastResolution * 0.01)))
        {
            return;
        }

        lastResolution = resolution;

        var cellSize = resolution * CellPixels;
        var cells = new Dictionary<(long X, long Y), List<Mapsui.MPoint>>();
        foreach (var point in points)
        {
            var key = ((long)Math.Floor(point.X / cellSize), (long)Math.Floor(point.Y / cellSize));
            if (!cells.TryGetValue(key, out var list))
            {
                list = [];
                cells[key] = list;
            }

            list.Add(point);
        }

        var features = new List<Mapsui.IFeature>();
        foreach (var cell in cells.Values)
        {
            var cx = cell.Average(static p => p.X);
            var cy = cell.Average(static p => p.Y);
            var feature = new PointFeature(cx, cy);
            if (cell.Count > 1)
            {
                feature.Styles.Add(new SymbolStyle
                {
                    SymbolType = SymbolType.Ellipse,
                    SymbolScale = Math.Min(1.4, 0.7 + (cell.Count * 0.02)),
                    Fill = new MapsuiBrush(new MapsuiColor(0x1E, 0x88, 0xE5, 0xCC)),
                    Outline = new MapsuiPen(MapsuiColor.White, 2)
                });
                feature.Styles.Add(new LabelStyle
                {
                    Text = cell.Count.ToString(),
                    ForeColor = MapsuiColor.White,
                    BackColor = null,
                    Font = new Font { Size = 12, Bold = true }
                });
            }
            else
            {
                feature.Styles.Add(new SymbolStyle
                {
                    SymbolType = SymbolType.Ellipse,
                    SymbolScale = 0.4,
                    Fill = new MapsuiBrush(MapsuiColor.FromString("#43A047")),
                    Outline = new MapsuiPen(MapsuiColor.White, 1)
                });
            }

            features.Add(feature);
        }

        layer.Features = features;
        layer.DataHasChanged();
    }
}
#pragma warning restore CA5394
