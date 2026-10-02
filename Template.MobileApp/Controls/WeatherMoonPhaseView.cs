namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

// 月の満ち欠け。Age は新月からの日数。満ちていくときは右、欠けていくときは左が光る
public sealed class WeatherMoonPhaseView : SKCanvasView
{
    private const double SynodicMonth = 29.530588853;

    public static readonly BindableProperty AgeProperty = BindableProperty.Create(
        nameof(Age),
        typeof(double),
        typeof(WeatherMoonPhaseView),
        0d,
        propertyChanged: OnVisualChanged);

    public double Age
    {
        get => (double)GetValue(AgeProperty);
        set => SetValue(AgeProperty, value);
    }

    public static readonly BindableProperty LightColorProperty = BindableProperty.Create(
        nameof(LightColor),
        typeof(Color),
        typeof(WeatherMoonPhaseView),
        Color.FromArgb("#FFF4C7"),
        propertyChanged: OnVisualChanged);

    public Color LightColor
    {
        get => (Color)GetValue(LightColorProperty);
        set => SetValue(LightColorProperty, value);
    }

    public static readonly BindableProperty DarkColorProperty = BindableProperty.Create(
        nameof(DarkColor),
        typeof(Color),
        typeof(WeatherMoonPhaseView),
        Color.FromRgba(255, 255, 255, 36),
        propertyChanged: OnVisualChanged);

    public Color DarkColor
    {
        get => (Color)GetValue(DarkColorProperty);
        set => SetValue(DarkColorProperty, value);
    }

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((WeatherMoonPhaseView)bindable).InvalidateSurface();

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
            var radius = (MathF.Min(info.Width, info.Height) / 2f) - (8 * density);
            var phase = (float)((((Age % SynodicMonth) + SynodicMonth) % SynodicMonth) / SynodicMonth);
            var light = LightColor.ToSKColor();

            using var glow = new SKPaint();
            glow.IsAntialias = true;
            glow.Color = light.WithAlpha((byte)(30 + (70 * (1 - MathF.Cos(phase * 2 * MathF.PI)) / 2)));
            glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6 * density);
            canvas.DrawCircle(center, radius + (2 * density), glow);

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Color = DarkColor.ToSKColor();
            canvas.DrawCircle(center, radius, paint);

            using var path = CreateLitPath(center, radius, phase);
            using var shader = SKShader.CreateRadialGradient(
                new SKPoint(center.X - (radius * 0.3f), center.Y - (radius * 0.3f)),
                radius * 1.6f,
                [light, light.WithAlpha(210)],
                SKShaderTileMode.Clamp);
            paint.Shader = shader;
            canvas.DrawPath(path, paint);
            paint.Shader = null;

            // 光っている部分の模様 (海)
            canvas.Save();
            canvas.ClipPath(path, antialias: true);
            paint.Color = SKColors.Black.WithAlpha(22);
            canvas.DrawCircle(center.X - (radius * 0.28f), center.Y - (radius * 0.22f), radius * 0.22f, paint);
            canvas.DrawCircle(center.X + (radius * 0.25f), center.Y + (radius * 0.05f), radius * 0.28f, paint);
            canvas.DrawCircle(center.X - (radius * 0.05f), center.Y + (radius * 0.42f), radius * 0.16f, paint);
            canvas.Restore();
        }
    }

    // 光っている側の半円と、明暗の境目の半楕円 (幅は位相で変わる) で囲む
    private static SKPath CreateLitPath(SKPoint center, float radius, float phase)
    {
        var circle = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
        var half = radius * MathF.Abs(MathF.Cos(phase * 2 * MathF.PI));
        var terminator = new SKRect(center.X - half, center.Y - radius, center.X + half, center.Y + radius);
        var waxing = phase < 0.5f;
        var crescent = (phase < 0.25f) || (phase >= 0.75f);

        using var builder = new SKPathBuilder();
        builder.MoveTo(center.X, center.Y - radius);
        builder.ArcTo(circle, -90, waxing ? 180 : -180, false);

        // 三日月は境目が光る側へふくらみ、それ以外は反対側へふくらむ
        var towardRight = waxing == crescent;
        builder.ArcTo(terminator, 90, towardRight ? -180 : 180, false);
        builder.Close();
        return builder.Detach();
    }
}
