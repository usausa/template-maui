namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

public sealed class RangeBar : SKCanvasView
{
    private const float EdgeMargin = 5f;

    private const float ShadowBlur = 1.5f;

    private const float ShadowSpread = 1.5f;

    private const float MarkerSpread = 1f;

    private const byte ShadowAlpha = 90;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(RangeBar),
        0d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(RangeBar),
        1d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty LowProperty = BindableProperty.Create(
        nameof(Low),
        typeof(double),
        typeof(RangeBar),
        0d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty HighProperty = BindableProperty.Create(
        nameof(High),
        typeof(double),
        typeof(RangeBar),
        1d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty MarkerProperty = BindableProperty.Create(
        nameof(Marker),
        typeof(double?),
        typeof(RangeBar),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(RangeBar),
        Color.FromRgba(0, 0, 0, 40),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty StartColorProperty = BindableProperty.Create(
        nameof(StartColor),
        typeof(Color),
        typeof(RangeBar),
        Colors.DeepSkyBlue,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty EndColorProperty = BindableProperty.Create(
        nameof(EndColor),
        typeof(Color),
        typeof(RangeBar),
        Colors.Orange,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty MarkerColorProperty = BindableProperty.Create(
        nameof(MarkerColor),
        typeof(Color),
        typeof(RangeBar),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty ThicknessProperty = BindableProperty.Create(
        nameof(Thickness),
        typeof(double),
        typeof(RangeBar),
        6d,
        propertyChanged: OnVisualChanged);

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Low
    {
        get => (double)GetValue(LowProperty);
        set => SetValue(LowProperty, value);
    }

    public double High
    {
        get => (double)GetValue(HighProperty);
        set => SetValue(HighProperty, value);
    }

    public double? Marker
    {
        get => (double?)GetValue(MarkerProperty);
        set => SetValue(MarkerProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
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

    public Color MarkerColor
    {
        get => (Color)GetValue(MarkerColorProperty);
        set => SetValue(MarkerColorProperty, value);
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
        ((RangeBar)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            var density = (float)(info.Width / Width);
            var thickness = (float)Thickness * density;
            var radius = thickness / 2;
            var centerY = info.Height / 2f;

            var left = radius + (EdgeMargin * density);
            var right = info.Width - left;

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Color = TrackColor.ToSKColor();
            canvas.DrawRoundRect(new SKRect(left - radius, centerY - radius, right + radius, centerY + radius), radius, radius, paint);

            var low = ToX(Math.Min(Low, High), left, right);
            var high = ToX(Math.Max(Low, High), left, right);
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(left, 0),
                new SKPoint(right, 0),
                [StartColor.ToSKColor(), EndColor.ToSKColor()],
                SKShaderTileMode.Clamp);
            paint.Color = SKColors.White;
            paint.Shader = shader;
            canvas.DrawRoundRect(new SKRect(low - radius, centerY - radius, high + radius, centerY + radius), radius, radius, paint);
            paint.Shader = null;

            if (Marker is { } marker)
            {
                var x = ToX(marker, left, right);
                using var shadow = new SKPaint();
                shadow.IsAntialias = true;
                shadow.Color = SKColors.Black.WithAlpha(ShadowAlpha);
                shadow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, ShadowBlur * density);
                canvas.DrawCircle(x, centerY, radius + (ShadowSpread * density), shadow);
                paint.Color = MarkerColor.ToSKColor();
                canvas.DrawCircle(x, centerY, radius + (MarkerSpread * density), paint);
            }
        }
    }

    private float ToX(double value, float left, float right)
    {
        var range = Maximum - Minimum;
        var ratio = range > 0 ? Math.Clamp((value - Minimum) / range, 0, 1) : 0.5;
        return left + (float)(ratio * (right - left));
    }
}
