namespace Template.MobileApp.Messaging;

using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.UI.Maui;

using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

public interface IMapsuiController
{
    void Attach(MapControl view);

    void Detach();

    void AttachOverlay(SKCanvasView view);

    void DetachOverlay(SKCanvasView view);
}

// 機能グループ別マネージャ (IMapsuiMapManager) を辞書に登録し、個別に有効/無効化するコントローラ。
// SkiaSharp オーバーレイ (地図上へのグラデーション経路描画) も本クラスが仲介する
public sealed class MapsuiController : IMapsuiController
{
    private readonly double homeLongitude;

    private readonly double homeLatitude;

    private readonly int? initialResolution;

    // 機能グループ別マネージャの登録辞書 (Mapper)
    private readonly Dictionary<string, IMapsuiMapManager> managers = [];

    private readonly HashSet<string> enabledManagers = [];

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

        // 有効化済みマネージャを適用
        foreach (var name in enabledManagers)
        {
            if (managers.TryGetValue(name, out var manager))
            {
                manager.Attach(map.Map);
            }
        }

        // 地図の移動/ズームに合わせてオーバーレイを再描画する
        // (Navigator は Map と共に破棄されるため購読解除は行わない)
        map.Map.Navigator.ViewportChanged += (_, _) => overlayView?.InvalidateSurface();
    }

    void IMapsuiController.Detach()
    {
        if (map is not null)
        {
            foreach (var name in enabledManagers)
            {
                if (managers.TryGetValue(name, out var manager))
                {
                    manager.Detach(map.Map);
                }
            }
        }

        map = null;
    }

    //--------------------------------------------------------------------------------
    // Manager
    //--------------------------------------------------------------------------------

    public void AddManager(IMapsuiMapManager manager)
    {
        managers[manager.Name] = manager;
    }

    public void SetManagerEnabled(string name, bool enabled)
    {
        if (enabled ? !enabledManagers.Add(name) : !enabledManagers.Remove(name))
        {
            return;
        }

        if ((map is not null) && managers.TryGetValue(name, out var manager))
        {
            if (enabled)
            {
                manager.Attach(map.Map);
            }
            else
            {
                manager.Detach(map.Map);
            }

            map.Map.RefreshData();
        }
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // Overlay (SkiaSharp によるグラデーション経路)
    //--------------------------------------------------------------------------------

    private SKCanvasView? overlayView;

    private List<Mapsui.MPoint>? overlayRoute;

    void IMapsuiController.AttachOverlay(SKCanvasView view)
    {
        overlayView = view;
        view.PaintSurface += OnOverlayPaintSurface;
    }

    void IMapsuiController.DetachOverlay(SKCanvasView view)
    {
        view.PaintSurface -= OnOverlayPaintSurface;
        overlayView = null;
    }

    // 経路 (経度緯度) を設定する。null で消去
    public void SetOverlayRoute(IReadOnlyList<(double Lon, double Lat)>? route)
    {
        overlayRoute = route?.Select(static p => SphericalMercator.FromLonLat(p.Lon, p.Lat).ToMPoint()).ToList();
        overlayView?.InvalidateSurface();
    }

    private void OnOverlayPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var view = overlayView;
        var route = overlayRoute;
        if ((map is null) || (view is null) || (route is null) || (route.Count < 2) || (view.Width <= 0))
        {
            return;
        }

        // ビューポートは論理座標系のため、物理ピクセルとの倍率を合わせる
        var scale = (float)(e.Info.Width / view.Width);
        canvas.Scale(scale);

        var viewport = map.Map.Navigator.Viewport;
        var points = new SKPoint[route.Count];
        for (var i = 0; i < route.Count; i++)
        {
            var screen = viewport.WorldToScreen(route[i]);
            points[i] = new SKPoint((float)screen.X, (float)screen.Y);
        }

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeCap = SKStrokeCap.Round;

        // 白のハロー
        paint.Color = SKColors.White.WithAlpha(180);
        paint.StrokeWidth = 12f;
        for (var i = 1; i < points.Length; i++)
        {
            canvas.DrawLine(points[i - 1], points[i], paint);
        }

        // 線分ごとに始点→終点の色を補間したグラデーションシェーダを割り当てる (Run Away! app の技法)
        var start = new SKColor(0x1E, 0x88, 0xE5);
        var end = new SKColor(0xE5, 0x39, 0x35);
        paint.StrokeWidth = 7f;
        for (var i = 1; i < points.Length; i++)
        {
            var c0 = Lerp(start, end, (i - 1) / (float)(points.Length - 1));
            var c1 = Lerp(start, end, i / (float)(points.Length - 1));
            using var shader = SKShader.CreateLinearGradient(points[i - 1], points[i], [c0, c1], null, SKShaderTileMode.Clamp);
            paint.Shader = shader;
            canvas.DrawLine(points[i - 1], points[i], paint);
            paint.Shader = null;
        }
    }

    private static SKColor Lerp(SKColor from, SKColor to, float t) =>
        new(
            (byte)(from.Red + ((to.Red - from.Red) * t)),
            (byte)(from.Green + ((to.Green - from.Green) * t)),
            (byte)(from.Blue + ((to.Blue - from.Blue) * t)));
}
