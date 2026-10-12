namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

public sealed class ArcMeter : SKCanvasView
{
    private const float ArcPadding = 4f;

    private const int FitSteps = 72;

    private const float MinimumSpan = 0.01f;

    private const float TickStrokeWidth = 1.5f;

    private const float TickOuterInset = 3f;

    private const float TickInnerInset = 9f;

    private const float ShadowBlur = 1.5f;

    private const float ShadowSpread = 1.5f;

    private const float MarkerSpread = 1f;

    private const float NeedleInsetRate = 1.6f;

    private const float NeedleWidth = 3f;

    private const float PivotRadius = 5f;

    private const byte ShadowAlpha = 90;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(ArcMeter),
        0d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(ArcMeter),
        1d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(ArcMeter),
        0d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty StartAngleProperty = BindableProperty.Create(
        nameof(StartAngle),
        typeof(float),
        typeof(ArcMeter),
        135f,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty SweepAngleProperty = BindableProperty.Create(
        nameof(SweepAngle),
        typeof(float),
        typeof(ArcMeter),
        270f,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty ThicknessProperty = BindableProperty.Create(
        nameof(Thickness),
        typeof(double),
        typeof(ArcMeter),
        10d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(ArcMeter),
        Color.FromRgba(255, 255, 255, 50),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty BrushProperty = BindableProperty.Create(
        nameof(Brush),
        typeof(Brush),
        typeof(ArcMeter),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty FillProperty = BindableProperty.Create(
        nameof(Fill),
        typeof(bool),
        typeof(ArcMeter),
        true,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty ShowNeedleProperty = BindableProperty.Create(
        nameof(ShowNeedle),
        typeof(bool),
        typeof(ArcMeter),
        false,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty NeedleColorProperty = BindableProperty.Create(
        nameof(NeedleColor),
        typeof(Color),
        typeof(ArcMeter),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TickCountProperty = BindableProperty.Create(
        nameof(TickCount),
        typeof(int),
        typeof(ArcMeter),
        0,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TickColorProperty = BindableProperty.Create(
        nameof(TickColor),
        typeof(Color),
        typeof(ArcMeter),
        Color.FromRgba(255, 255, 255, 120),
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

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public float StartAngle
    {
        get => (float)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public float SweepAngle
    {
        get => (float)GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    public double Thickness
    {
        get => (double)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public Brush? Brush
    {
        get => (Brush?)GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public bool Fill
    {
        get => (bool)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public bool ShowNeedle
    {
        get => (bool)GetValue(ShowNeedleProperty);
        set => SetValue(ShowNeedleProperty, value);
    }

    public Color NeedleColor
    {
        get => (Color)GetValue(NeedleColorProperty);
        set => SetValue(NeedleColorProperty, value);
    }

    public int TickCount
    {
        get => (int)GetValue(TickCountProperty);
        set => SetValue(TickCountProperty, value);
    }

    public Color TickColor
    {
        get => (Color)GetValue(TickColorProperty);
        set => SetValue(TickColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((ArcMeter)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            var density = (float)(info.Width / Width);
            var thickness = (float)Thickness * density;
            var (center, radius) = FitArc(info.Width, info.Height, (thickness / 2) + (ArcPadding * density));
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
            canvas.DrawArc(center, radius, StartAngle, SweepAngle, false, paint);

            using var shader = CreateShader(center);
            paint.Color = shader is null ? BrushColor() : SKColors.White;
            paint.Shader = shader;
            if (Fill)
            {
                if (ratio > 0)
                {
                    canvas.DrawArc(center, radius, StartAngle, SweepAngle * ratio, false, paint);
                }
            }
            else
            {
                canvas.DrawArc(center, radius, StartAngle, SweepAngle, false, paint);
                DrawMarker(canvas, AngleHelper.GetPoint(center, radius, valueAngle), thickness, density);
            }

            if (ShowNeedle)
            {
                DrawNeedle(canvas, center, radius - (thickness * NeedleInsetRate), valueAngle, density);
            }
        }
    }

    private (SKPoint Center, float Radius) FitArc(int width, int height, float padding)
    {
        var minX = ShowNeedle ? 0f : Single.MaxValue;
        var maxX = ShowNeedle ? 0f : Single.MinValue;
        var minY = minX;
        var maxY = maxX;
        for (var i = 0; i <= FitSteps; i++)
        {
            var point = AngleHelper.GetPoint(SKPoint.Empty, 1f, StartAngle + (SweepAngle * i / FitSteps));
            minX = MathF.Min(minX, point.X);
            maxX = MathF.Max(maxX, point.X);
            minY = MathF.Min(minY, point.Y);
            maxY = MathF.Max(maxY, point.Y);
        }

        var spanX = MathF.Max(maxX - minX, MinimumSpan);
        var spanY = MathF.Max(maxY - minY, MinimumSpan);
        var radius = MathF.Min((width - (padding * 2)) / spanX, (height - (padding * 2)) / spanY);
        var left = (width - (spanX * radius)) / 2;
        var top = (height - (spanY * radius)) / 2;
        return (new SKPoint(left - (minX * radius), top - (minY * radius)), radius);
    }

    private SKShader? CreateShader(SKPoint center)
    {
        if (Brush is not GradientBrush { GradientStops.Count: > 1 } gradient)
        {
            return null;
        }

        var stops = gradient.GradientStops;
        var colors = new SKColor[stops.Count];
        var positions = new float[stops.Count];
        for (var i = 0; i < stops.Count; i++)
        {
            var stop = stops[i];
            var index = i;
            while ((index > 0) && (positions[index - 1] > stop.Offset))
            {
                colors[index] = colors[index - 1];
                positions[index] = positions[index - 1];
                index--;
            }

            colors[index] = stop.Color.ToSKColor();
            positions[index] = stop.Offset;
        }

        return AngleHelper.CreateArcGradient(center, StartAngle, SweepAngle, colors, positions);
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
            paint.StrokeWidth = TickStrokeWidth * density;
            paint.StrokeCap = SKStrokeCap.Round;
            paint.Color = TickColor.ToSKColor();
            for (var i = 0; i < TickCount; i++)
            {
                var angle = StartAngle + (SweepAngle * i / (TickCount - 1));
                canvas.DrawLine(AngleHelper.GetPoint(center, radius - (TickOuterInset * density), angle), AngleHelper.GetPoint(center, radius - (TickInnerInset * density), angle), paint);
            }
        }
    }

    private static void DrawMarker(SKCanvas canvas, SKPoint point, float thickness, float density)
    {
        using var shadow = new SKPaint();
        shadow.IsAntialias = true;
        shadow.Color = SKColors.Black.WithAlpha(ShadowAlpha);
        shadow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, ShadowBlur * density);
        canvas.DrawCircle(point, (thickness / 2) + (ShadowSpread * density), shadow);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = SKColors.White;
        canvas.DrawCircle(point, (thickness / 2) + (MarkerSpread * density), paint);
    }

    private void DrawNeedle(SKCanvas canvas, SKPoint center, float length, float angle, float density)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = NeedleWidth * density;
        paint.StrokeCap = SKStrokeCap.Round;
        paint.Color = NeedleColor.ToSKColor();
        canvas.DrawLine(center, AngleHelper.GetPoint(center, length, angle), paint);

        paint.Style = SKPaintStyle.Fill;
        canvas.DrawCircle(center, PivotRadius * density, paint);
    }
}
