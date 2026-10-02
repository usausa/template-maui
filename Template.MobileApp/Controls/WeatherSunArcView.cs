namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

// 日の出から日の入りまでの太陽の道のり。通った部分を塗り、今の位置に太陽を置く (夜は地平線の端で暗く)
public sealed class WeatherSunArcView : SKCanvasView
{
    public static readonly BindableProperty SunriseProperty = BindableProperty.Create(
        nameof(Sunrise),
        typeof(DateTime),
        typeof(WeatherSunArcView),
        default(DateTime),
        propertyChanged: OnVisualChanged);

    public DateTime Sunrise
    {
        get => (DateTime)GetValue(SunriseProperty);
        set => SetValue(SunriseProperty, value);
    }

    public static readonly BindableProperty SunsetProperty = BindableProperty.Create(
        nameof(Sunset),
        typeof(DateTime),
        typeof(WeatherSunArcView),
        default(DateTime),
        propertyChanged: OnVisualChanged);

    public DateTime Sunset
    {
        get => (DateTime)GetValue(SunsetProperty);
        set => SetValue(SunsetProperty, value);
    }

    public static readonly BindableProperty NowProperty = BindableProperty.Create(
        nameof(Now),
        typeof(DateTime),
        typeof(WeatherSunArcView),
        default(DateTime),
        propertyChanged: OnVisualChanged);

    public DateTime Now
    {
        get => (DateTime)GetValue(NowProperty);
        set => SetValue(NowProperty, value);
    }

    public static readonly BindableProperty PathColorProperty = BindableProperty.Create(
        nameof(PathColor),
        typeof(Color),
        typeof(WeatherSunArcView),
        Color.FromRgba(255, 255, 255, 90),
        propertyChanged: OnVisualChanged);

    public Color PathColor
    {
        get => (Color)GetValue(PathColorProperty);
        set => SetValue(PathColorProperty, value);
    }

    public static readonly BindableProperty SunColorProperty = BindableProperty.Create(
        nameof(SunColor),
        typeof(Color),
        typeof(WeatherSunArcView),
        Color.FromArgb("#FFD54F"),
        propertyChanged: OnVisualChanged);

    public Color SunColor
    {
        get => (Color)GetValue(SunColorProperty);
        set => SetValue(SunColorProperty, value);
    }

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((WeatherSunArcView)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0) && (Sunset > Sunrise))
        {
            // SKCanvas は物理ピクセルなので、論理単位の太さや長さに密度を掛ける
            var density = (float)(info.Width / Width);
            var sun = 7 * density;
            var margin = sun + (6 * density);
            var horizon = info.Height - margin;

            // 地平線から上の半分の楕円
            var oval = new SKRect(margin, margin, info.Width - margin, (horizon * 2) - margin);
            var progress = (float)Math.Clamp((Now - Sunrise).TotalMinutes / (Sunset - Sunrise).TotalMinutes, 0, 1);
            var isDay = (Now > Sunrise) && (Now < Sunset);
            var sunColor = SunColor.ToSKColor();

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.5f * density;
            paint.Color = PathColor.ToSKColor();
            paint.PathEffect = SKPathEffect.CreateDash([4 * density, 4 * density], 0);
            DrawArc(canvas, oval, 180, 180, paint);
            paint.PathEffect = null;

            // 通った部分の下を塗り、線を太くする
            if (progress > 0)
            {
                using var fill = new SKPaint();
                fill.IsAntialias = true;
                using var shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, margin),
                    new SKPoint(0, horizon),
                    [sunColor.WithAlpha(110), sunColor.WithAlpha(0)],
                    SKShaderTileMode.Clamp);
                fill.Shader = shader;
                using var builder = new SKPathBuilder();
                builder.AddArc(oval, 180, 180 * progress);
                var end = PointAt(oval, 180 + (180 * progress));
                builder.LineTo(end.X, horizon);
                builder.LineTo(oval.Left, horizon);
                builder.Close();
                using var path = builder.Detach();
                canvas.DrawPath(path, fill);

                paint.StrokeWidth = 3 * density;
                paint.StrokeCap = SKStrokeCap.Round;
                paint.Color = sunColor;
                DrawArc(canvas, oval, 180, 180 * progress, paint);
            }

            paint.StrokeWidth = 1 * density;
            paint.Color = PathColor.ToSKColor();
            canvas.DrawLine(0, horizon, info.Width, horizon, paint);

            var position = PointAt(oval, 180 + (180 * progress));
            using var glow = new SKPaint();
            glow.IsAntialias = true;
            glow.Color = sunColor.WithAlpha(isDay ? (byte)150 : (byte)60);
            glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5 * density);
            canvas.DrawCircle(position, sun * 1.4f, glow);

            paint.Style = SKPaintStyle.Fill;
            paint.Color = isDay ? sunColor : sunColor.WithAlpha(110);
            canvas.DrawCircle(position, sun, paint);
        }
    }

    private static SKPoint PointAt(SKRect oval, float angle)
    {
        var radian = angle * MathF.PI / 180f;
        return new SKPoint(oval.MidX + (MathF.Cos(radian) * oval.Width / 2), oval.MidY + (MathF.Sin(radian) * oval.Height / 2));
    }

    private static void DrawArc(SKCanvas canvas, SKRect oval, float start, float sweep, SKPaint paint)
    {
        using var builder = new SKPathBuilder();
        builder.AddArc(oval, start, sweep);
        using var path = builder.Detach();
        canvas.DrawPath(path, paint);
    }
}
