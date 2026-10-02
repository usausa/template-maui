namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

// 風の羅針盤。Direction は風が吹いてくる向き (度。北 = 0 で時計回り) で、矢印は風下を向く。
// 真ん中は文字を重ねるために空ける
public sealed class WeatherCompassDial : SKCanvasView
{
    private static readonly SKTypeface BoldTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

    private static readonly string[] Labels = ["N", "E", "S", "W"];

    public static readonly BindableProperty DirectionProperty = BindableProperty.Create(
        nameof(Direction),
        typeof(double),
        typeof(WeatherCompassDial),
        0d,
        propertyChanged: OnVisualChanged);

    public double Direction
    {
        get => (double)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public static readonly BindableProperty TickColorProperty = BindableProperty.Create(
        nameof(TickColor),
        typeof(Color),
        typeof(WeatherCompassDial),
        Color.FromRgba(255, 255, 255, 110),
        propertyChanged: OnVisualChanged);

    public Color TickColor
    {
        get => (Color)GetValue(TickColorProperty);
        set => SetValue(TickColorProperty, value);
    }

    public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(
        nameof(LabelColor),
        typeof(Color),
        typeof(WeatherCompassDial),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public Color LabelColor
    {
        get => (Color)GetValue(LabelColorProperty);
        set => SetValue(LabelColorProperty, value);
    }

    public static readonly BindableProperty NorthColorProperty = BindableProperty.Create(
        nameof(NorthColor),
        typeof(Color),
        typeof(WeatherCompassDial),
        Color.FromArgb("#FF8A80"),
        propertyChanged: OnVisualChanged);

    public Color NorthColor
    {
        get => (Color)GetValue(NorthColorProperty);
        set => SetValue(NorthColorProperty, value);
    }

    public static readonly BindableProperty ArrowColorProperty = BindableProperty.Create(
        nameof(ArrowColor),
        typeof(Color),
        typeof(WeatherCompassDial),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public Color ArrowColor
    {
        get => (Color)GetValue(ArrowColorProperty);
        set => SetValue(ArrowColorProperty, value);
    }

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((WeatherCompassDial)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            // SKCanvas は物理ピクセルなので、論理単位の太さや長さに密度を掛ける
            var density = (float)(info.Width / Width);
            var center = new SKPoint(info.Width / 2f, info.Height / 2f);
            var radius = (MathF.Min(info.Width, info.Height) / 2f) - (4 * density);

            DrawTicks(canvas, center, radius, density);
            DrawLabels(canvas, center, radius - (20 * density), density);
            DrawArrow(canvas, center, radius - (12 * density), density);
        }
    }

    // 羅針盤の角度 (北 = 0 で時計回り) の位置
    private static SKPoint PointAt(SKPoint center, float radius, double degree)
    {
        var radian = (float)((degree - 90) * Math.PI / 180);
        return new SKPoint(center.X + (MathF.Cos(radian) * radius), center.Y + (MathF.Sin(radian) * radius));
    }

    // 5 度ごとの目盛り。30 度ごとに長く明るく
    private void DrawTicks(SKCanvas canvas, SKPoint center, float radius, float density)
    {
        var color = TickColor.ToSKColor();
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeCap = SKStrokeCap.Round;
        for (var degree = 0; degree < 360; degree += 5)
        {
            var major = (degree % 30) == 0;
            paint.StrokeWidth = (major ? 2f : 1f) * density;
            paint.Color = major ? color : color.WithAlpha((byte)(color.Alpha / 2));
            canvas.DrawLine(PointAt(center, radius, degree), PointAt(center, radius - ((major ? 7 : 4) * density), degree), paint);
        }
    }

    private void DrawLabels(SKCanvas canvas, SKPoint center, float radius, float density)
    {
        using var font = new SKFont(BoldTypeface, 12 * density);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        for (var i = 0; i < Labels.Length; i++)
        {
            var point = PointAt(center, radius, i * 90);
            paint.Color = (i == 0 ? NorthColor : LabelColor).ToSKColor();
            var baseline = point.Y - ((font.Metrics.Ascent + font.Metrics.Descent) / 2);
            canvas.DrawText(Labels[i], point.X, baseline, SKTextAlign.Center, font, paint);
        }
    }

    // 風上の丸から風下の矢じりまで。真ん中は空ける
    private void DrawArrow(SKCanvas canvas, SKPoint center, float radius, float density)
    {
        var from = Direction;
        var to = Direction + 180;
        var gap = radius * 0.45f;

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 2.5f * density;
        paint.StrokeCap = SKStrokeCap.Round;
        paint.Color = ArrowColor.ToSKColor();
        var tail = PointAt(center, radius - (4 * density), from);
        canvas.DrawLine(tail, PointAt(center, gap, from), paint);
        var head = PointAt(center, radius, to);
        canvas.DrawLine(PointAt(center, gap, to), PointAt(center, radius - (8 * density), to), paint);

        paint.Style = SKPaintStyle.Fill;
        canvas.DrawCircle(tail, 3.5f * density, paint);

        // 矢じり
        var back = PointAt(center, radius - (12 * density), to);
        var normal = (float)(to * Math.PI / 180);
        var offset = new SKPoint(MathF.Cos(normal) * 6 * density, MathF.Sin(normal) * 6 * density);
        using var builder = new SKPathBuilder();
        builder.MoveTo(head);
        builder.LineTo(back.X + offset.X, back.Y + offset.Y);
        builder.LineTo(back.X - offset.X, back.Y - offset.Y);
        builder.Close();
        using var path = builder.Detach();
        canvas.DrawPath(path, paint);
    }
}
