namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

// 円弧のメーター。角度は 3 時の方向が 0 で時計回り。円弧は大きさいっぱいに収める。
// Fill は最小から値までを Brush で塗り、Fill でないときは円弧の全体を Brush で塗って値の位置に印を置く
public sealed class ArcMeter : SKCanvasView
{
    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(ArcMeter),
        0d,
        propertyChanged: OnVisualChanged);

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(ArcMeter),
        1d,
        propertyChanged: OnVisualChanged);

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(ArcMeter),
        0d,
        propertyChanged: OnVisualChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty StartAngleProperty = BindableProperty.Create(
        nameof(StartAngle),
        typeof(float),
        typeof(ArcMeter),
        135f,
        propertyChanged: OnVisualChanged);

    public float StartAngle
    {
        get => (float)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public static readonly BindableProperty SweepAngleProperty = BindableProperty.Create(
        nameof(SweepAngle),
        typeof(float),
        typeof(ArcMeter),
        270f,
        propertyChanged: OnVisualChanged);

    public float SweepAngle
    {
        get => (float)GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    public static readonly BindableProperty ThicknessProperty = BindableProperty.Create(
        nameof(Thickness),
        typeof(double),
        typeof(ArcMeter),
        10d,
        propertyChanged: OnVisualChanged);

    public double Thickness
    {
        get => (double)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(ArcMeter),
        Color.FromRgba(255, 255, 255, 50),
        propertyChanged: OnVisualChanged);

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    // 単色かグラデーション (GradientStop の Offset は円弧の始まりから終わりまで)
    public static readonly BindableProperty BrushProperty = BindableProperty.Create(
        nameof(Brush),
        typeof(Brush),
        typeof(ArcMeter),
        propertyChanged: OnVisualChanged);

    public Brush? Brush
    {
        get => (Brush?)GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public static readonly BindableProperty FillProperty = BindableProperty.Create(
        nameof(Fill),
        typeof(bool),
        typeof(ArcMeter),
        true,
        propertyChanged: OnVisualChanged);

    public bool Fill
    {
        get => (bool)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public static readonly BindableProperty ShowNeedleProperty = BindableProperty.Create(
        nameof(ShowNeedle),
        typeof(bool),
        typeof(ArcMeter),
        false,
        propertyChanged: OnVisualChanged);

    public bool ShowNeedle
    {
        get => (bool)GetValue(ShowNeedleProperty);
        set => SetValue(ShowNeedleProperty, value);
    }

    public static readonly BindableProperty NeedleColorProperty = BindableProperty.Create(
        nameof(NeedleColor),
        typeof(Color),
        typeof(ArcMeter),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public Color NeedleColor
    {
        get => (Color)GetValue(NeedleColorProperty);
        set => SetValue(NeedleColorProperty, value);
    }

    // 円弧の内側の目盛りの数 (両端を含む。2 未満で出さない)
    public static readonly BindableProperty TickCountProperty = BindableProperty.Create(
        nameof(TickCount),
        typeof(int),
        typeof(ArcMeter),
        0,
        propertyChanged: OnVisualChanged);

    public int TickCount
    {
        get => (int)GetValue(TickCountProperty);
        set => SetValue(TickCountProperty, value);
    }

    public static readonly BindableProperty TickColorProperty = BindableProperty.Create(
        nameof(TickColor),
        typeof(Color),
        typeof(ArcMeter),
        Color.FromRgba(255, 255, 255, 120),
        propertyChanged: OnVisualChanged);

    public Color TickColor
    {
        get => (Color)GetValue(TickColorProperty);
        set => SetValue(TickColorProperty, value);
    }

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((ArcMeter)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            // SKCanvas は物理ピクセルなので、論理単位の太さや長さに密度を掛ける
            var density = (float)(info.Width / Width);
            var thickness = (float)Thickness * density;
            var (center, radius) = FitArc(info.Width, info.Height, (thickness / 2) + (4 * density));
            var rect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
            var range = Maximum - Minimum;
            var ratio = range > 0 ? (float)Math.Clamp((Value - Minimum) / range, 0, 1) : 0f;
            var valueAngle = StartAngle + (SweepAngle * ratio);

            DrawTicks(canvas, center, radius - thickness, density);

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = thickness;
            paint.StrokeCap = SKStrokeCap.Round;
            paint.Color = TrackColor.ToSKColor();
            DrawArc(canvas, rect, StartAngle, SweepAngle, paint);

            using var shader = CreateShader(center);
            paint.Color = shader is null ? BrushColor() : SKColors.White;
            paint.Shader = shader;
            if (Fill)
            {
                if (ratio > 0)
                {
                    DrawArc(canvas, rect, StartAngle, SweepAngle * ratio, paint);
                }
            }
            else
            {
                DrawArc(canvas, rect, StartAngle, SweepAngle, paint);
                DrawMarker(canvas, PointAt(center, radius, valueAngle), thickness, density);
            }

            if (ShowNeedle)
            {
                DrawNeedle(canvas, center, radius - (thickness * 1.6f), valueAngle, density);
            }
        }
    }

    // 円弧 (と針の中心) の外形がいちばん大きく収まる中心と半径
    private (SKPoint Center, float Radius) FitArc(int width, int height, float padding)
    {
        var minX = ShowNeedle ? 0f : Single.MaxValue;
        var maxX = ShowNeedle ? 0f : Single.MinValue;
        var minY = minX;
        var maxY = maxX;
        var steps = 72;
        for (var i = 0; i <= steps; i++)
        {
            var angle = (StartAngle + (SweepAngle * i / steps)) * MathF.PI / 180f;
            var x = MathF.Cos(angle);
            var y = MathF.Sin(angle);
            minX = MathF.Min(minX, x);
            maxX = MathF.Max(maxX, x);
            minY = MathF.Min(minY, y);
            maxY = MathF.Max(maxY, y);
        }

        var spanX = MathF.Max(maxX - minX, 0.01f);
        var spanY = MathF.Max(maxY - minY, 0.01f);
        var radius = MathF.Min((width - (padding * 2)) / spanX, (height - (padding * 2)) / spanY);
        var left = (width - (spanX * radius)) / 2;
        var top = (height - (spanY * radius)) / 2;
        return (new SKPoint(left - (minX * radius), top - (minY * radius)), radius);
    }

    private static SKPoint PointAt(SKPoint center, float radius, float angle)
    {
        var radian = angle * MathF.PI / 180f;
        return new SKPoint(center.X + (MathF.Cos(radian) * radius), center.Y + (MathF.Sin(radian) * radius));
    }

    private static void DrawArc(SKCanvas canvas, SKRect rect, float start, float sweep, SKPaint paint)
    {
        using var builder = new SKPathBuilder();
        builder.AddArc(rect, start, sweep);
        using var path = builder.Detach();
        canvas.DrawPath(path, paint);
    }

    // 円弧の始まりを 0、終わりを 1 とするグラデーション (単色は null)
    private SKShader? CreateShader(SKPoint center)
    {
        if (Brush is not GradientBrush { GradientStops.Count: > 1 } gradient)
        {
            return null;
        }

        var stops = gradient.GradientStops.OrderBy(static x => x.Offset).ToArray();
        var colors = stops.Select(static x => x.Color.ToSKColor()).ToArray();
        var positions = stops.Select(x => x.Offset * SweepAngle / 360f).ToArray();
        using var shader = SKShader.CreateSweepGradient(center, colors, positions);
        return shader.WithLocalMatrix(SKMatrix.CreateRotationDegrees(StartAngle, center.X, center.Y));
    }

    private SKColor BrushColor() =>
        Brush is SolidColorBrush solid ? solid.Color.ToSKColor() : SKColors.White;

    private void DrawTicks(SKCanvas canvas, SKPoint center, float radius, float density)
    {
        if (TickCount >= 2)
        {
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.5f * density;
            paint.StrokeCap = SKStrokeCap.Round;
            paint.Color = TickColor.ToSKColor();
            for (var i = 0; i < TickCount; i++)
            {
                var angle = StartAngle + (SweepAngle * i / (TickCount - 1));
                canvas.DrawLine(PointAt(center, radius - (3 * density), angle), PointAt(center, radius - (9 * density), angle), paint);
            }
        }
    }

    // 影付きの白い印
    private static void DrawMarker(SKCanvas canvas, SKPoint point, float thickness, float density)
    {
        using var shadow = new SKPaint();
        shadow.IsAntialias = true;
        shadow.Color = SKColors.Black.WithAlpha(90);
        shadow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 1.5f * density);
        canvas.DrawCircle(point, (thickness / 2) + (1.5f * density), shadow);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = SKColors.White;
        canvas.DrawCircle(point, (thickness / 2) + density, paint);
    }

    private void DrawNeedle(SKCanvas canvas, SKPoint center, float length, float angle, float density)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 3 * density;
        paint.StrokeCap = SKStrokeCap.Round;
        paint.Color = NeedleColor.ToSKColor();
        canvas.DrawLine(center, PointAt(center, length, angle), paint);

        paint.Style = SKPaintStyle.Fill;
        canvas.DrawCircle(center, 5 * density, paint);
    }
}
