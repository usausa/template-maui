namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

public sealed class GlowRing : SKCanvasView
{
    private const float StartAngle = -90f;

    private const float BlurRate = 0.6f;

    private const float GlowInsetRate = 0.8f;

    private const float BlurInsetRate = 2f;

    private const float GlowWidthRate = 1.8f;

    private const byte GlowAlpha = 140;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
        nameof(Progress),
        typeof(double),
        typeof(GlowRing),
        0d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty StartColorProperty = BindableProperty.Create(
        nameof(StartColor),
        typeof(Color),
        typeof(GlowRing),
        Colors.Cyan,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty EndColorProperty = BindableProperty.Create(
        nameof(EndColor),
        typeof(Color),
        typeof(GlowRing),
        Colors.DodgerBlue,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(GlowRing),
        Color.FromRgba(255, 255, 255, 20),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty ThicknessProperty = BindableProperty.Create(
        nameof(Thickness),
        typeof(double),
        typeof(GlowRing),
        8d,
        propertyChanged: OnVisualChanged);

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public Color StartColor
    {
        get => (Color)GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }

    public Color EndColor
    {
        get => (Color)GetValue(EndColorProperty);
        set => SetValue(EndColorProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public double Thickness
    {
        get => (double)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((GlowRing)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            var density = (float)(info.Width / Width);
            var stroke = (float)Thickness * density;
            var blur = stroke * BlurRate;
            var radius = (MathF.Min(info.Width, info.Height) / 2f) - (stroke * GlowInsetRate) - (blur * BlurInsetRate);
            var center = new SKPoint(info.Width / 2f, info.Height / 2f);

            using var track = new SKPaint();
            track.IsAntialias = true;
            track.Style = SKPaintStyle.Stroke;
            track.StrokeWidth = stroke;
            track.Color = TrackColor.ToSKColor();
            canvas.DrawCircle(center, radius, track);

            DrawGlowArc(canvas, center, radius, stroke, blur, (float)Math.Clamp(Progress, 0d, 1d), StartColor.ToSKColor(), EndColor.ToSKColor());
        }
    }

    internal static void DrawGlowArc(SKCanvas canvas, SKPoint center, float radius, float stroke, float blur, float progress, SKColor startColor, SKColor endColor)
    {
        if (progress > 0f)
        {
            DrawArc(canvas, center, radius, stroke, blur, progress, startColor, endColor);
        }
    }

    private static void DrawArc(SKCanvas canvas, SKPoint center, float radius, float stroke, float blur, float progress, SKColor startColor, SKColor endColor)
    {
        var sweep = 360f * progress;
        using var shader = AngleHelper.CreateArcGradient(center, StartAngle, sweep, [startColor, endColor]);

        using var glow = new SKPaint();
        glow.IsAntialias = true;
        glow.Style = SKPaintStyle.Stroke;
        glow.StrokeWidth = stroke * GlowWidthRate;
        glow.StrokeCap = SKStrokeCap.Round;
        glow.Shader = shader;
        glow.Color = SKColors.White.WithAlpha(GlowAlpha);
        glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur);
        canvas.DrawArc(center, radius, StartAngle, sweep, false, glow);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = stroke;
        paint.StrokeCap = SKStrokeCap.Round;
        paint.Shader = shader;
        canvas.DrawArc(center, radius, StartAngle, sweep, false, paint);
    }
}
