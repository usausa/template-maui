namespace Template.MobileApp.Controls;

using SkiaSharp.Views.Maui;

//--------------------------------------------------------------------------------
// Base
//--------------------------------------------------------------------------------
public abstract class HomeControl : SKCanvasView
{
    protected static readonly BindableProperty.BindingPropertyChangedDelegate Invalidate =
        static (bindable, _, _) => ((HomeControl)bindable).InvalidateSurface();

    protected float GetDensity(SKImageInfo info) => Width > 0 ? (float)(info.Width / Width) : 1f;
}

//--------------------------------------------------------------------------------
// Background
//--------------------------------------------------------------------------------
public sealed partial class HomeBlurredImage : HomeControl
{
    private const int Reduction = 4;

    private const int SampleStep = 4;

    private const int BytesPerPixel = 4;

    private const double RedLuminance = 0.2126;

    private const double GreenLuminance = 0.7152;

    private const double BlueLuminance = 0.0722;

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private SKImage? cache;

    private (string? Source, int Width, int Height, double BlurRadius, double Dim, double MaxLuminance) cacheKey;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty SourceProperty = BindableProperty.Create(
        nameof(Source),
        typeof(string),
        typeof(HomeBlurredImage),
        propertyChanged: Invalidate);

    public static readonly BindableProperty BlurRadiusProperty = BindableProperty.Create(
        nameof(BlurRadius),
        typeof(double),
        typeof(HomeBlurredImage),
        16d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty DimProperty = BindableProperty.Create(
        nameof(Dim),
        typeof(double),
        typeof(HomeBlurredImage),
        0.5d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MaxLuminanceProperty = BindableProperty.Create(
        nameof(MaxLuminance),
        typeof(double),
        typeof(HomeBlurredImage),
        1d,
        propertyChanged: Invalidate);

    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public double BlurRadius
    {
        get => (double)GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    public double Dim
    {
        get => (double)GetValue(DimProperty);
        set => SetValue(DimProperty, value);
    }

    public double MaxLuminance
    {
        get => (double)GetValue(MaxLuminanceProperty);
        set => SetValue(MaxLuminanceProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.NewHandler is null)
        {
            cache?.Dispose();
            cache = null;
            cacheKey = default;
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColors.Black);

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            var key = (Source, info.Width, info.Height, BlurRadius, Dim, MaxLuminance);
            if (key != cacheKey)
            {
                cache?.Dispose();
                cache = Render(info, GetDensity(info));
                cacheKey = key;
            }

            if (cache is not null)
            {
                canvas.DrawImage(cache, info.Rect, new SKSamplingOptions(SKFilterMode.Linear));
            }
        }
    }

    private SKImage? Render(SKImageInfo info, float density)
    {
        using var bitmap = LoadBitmap(Source);
        if (bitmap is null)
        {
            return null;
        }

        var width = Math.Max(1, info.Width / Reduction);
        var height = Math.Max(1, info.Height / Reduction);
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        var scale = Math.Max(width / (float)bitmap.Width, height / (float)bitmap.Height);
        var dest = SKRect.Create((width - (bitmap.Width * scale)) / 2f, (height - (bitmap.Height * scale)) / 2f, bitmap.Width * scale, bitmap.Height * scale);
        var sigma = (float)BlurRadius * density / Reduction;
        using var paint = new SKPaint();
        paint.ImageFilter = SKImageFilter.CreateBlur(sigma, sigma, SKShaderTileMode.Clamp);
        using var image = SKImage.FromBitmap(bitmap);
        canvas.DrawImage(image, dest, new SKSamplingOptions(SKFilterMode.Linear), paint);

        var dim = Math.Clamp(Dim, 0d, 1d);
        if (MaxLuminance < 1d)
        {
            var luminance = MeasureLuminance(surface);
            if (luminance > 0d)
            {
                dim = Math.Max(dim, 1d - (MaxLuminance / luminance));
            }
        }

        canvas.DrawColor(SKColors.Black.WithOpacity((float)dim), SKBlendMode.SrcOver);
        return surface.Snapshot();
    }

    private static double MeasureLuminance(SKSurface surface)
    {
        using var pixmap = surface.PeekPixels();
        if ((pixmap is null) || ((pixmap.ColorType != SKColorType.Rgba8888) && (pixmap.ColorType != SKColorType.Bgra8888)))
        {
            return 0d;
        }

        var bgra = pixmap.ColorType == SKColorType.Bgra8888;
        var span = pixmap.GetPixelSpan();
        var sum = 0d;
        var count = 0;
        for (var y = 0; y < pixmap.Height; y += SampleStep)
        {
            var row = span.Slice(y * pixmap.RowBytes, pixmap.Width * BytesPerPixel);
            for (var x = 0; x < row.Length; x += SampleStep * BytesPerPixel)
            {
                var r = bgra ? row[x + 2] : row[x];
                var b = bgra ? row[x] : row[x + 2];
                sum += (RedLuminance * r) + (GreenLuminance * row[x + 1]) + (BlueLuminance * b);
                count++;
            }
        }

        return count > 0 ? sum / count / 255d : 0d;
    }

    //--------------------------------------------------------------------------------
    // Platform
    //--------------------------------------------------------------------------------

    private static partial SKBitmap? LoadBitmap(string? source);
}

//--------------------------------------------------------------------------------
// Thermostat
//--------------------------------------------------------------------------------
public sealed class HomeThermostatDial : HomeControl
{
    private const float StartAngle = 135f;

    private const float SweepAngle = 270f;

    private const float ArcStrokeWidth = 10f;

    private const float ArcInsetRate = 2.4f;

    private const float DiscInsetRate = 1.6f;

    private const float DiscLineWidth = 1f;

    private const float GlowWidthRate = 2f;

    private const float GlowBlur = 8f;

    private const float KnobGlowRate = 1.2f;

    private const float KnobGlowBlur = 6f;

    private const float KnobRingRate = 0.95f;

    private const float KnobDotRate = 0.6f;

    private const float TouchRangeRate = 3f;

    private const byte GlowAlpha = 150;

    private const byte KnobGlowAlpha = 200;

    private static readonly SKColor DiscTopColor = new(255, 255, 255, 40);

    private static readonly SKColor DiscBottomColor = new(255, 255, 255, 10);

    private static readonly SKColor DiscLineColor = new(255, 255, 255, 60);

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private bool dragging;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(HomeThermostatDial),
        0d,
        BindingMode.TwoWay,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(HomeThermostatDial),
        0d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(HomeThermostatDial),
        1d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty StepProperty = BindableProperty.Create(
        nameof(Step),
        typeof(double),
        typeof(HomeThermostatDial),
        0d);

    public static readonly BindableProperty ArcStartColorProperty = BindableProperty.Create(
        nameof(ArcStartColor),
        typeof(Color),
        typeof(HomeThermostatDial),
        Colors.Orange,
        propertyChanged: Invalidate);

    public static readonly BindableProperty ArcEndColorProperty = BindableProperty.Create(
        nameof(ArcEndColor),
        typeof(Color),
        typeof(HomeThermostatDial),
        Colors.OrangeRed,
        propertyChanged: Invalidate);

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(HomeThermostatDial),
        Color.FromRgba(255, 255, 255, 36),
        propertyChanged: Invalidate);

    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive),
        typeof(bool),
        typeof(HomeThermostatDial),
        true,
        propertyChanged: Invalidate);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

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

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public Color ArcStartColor
    {
        get => (Color)GetValue(ArcStartColorProperty);
        set => SetValue(ArcStartColorProperty, value);
    }

    public Color ArcEndColor
    {
        get => (Color)GetValue(ArcEndColorProperty);
        set => SetValue(ArcEndColorProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public HomeThermostatDial()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0))
        {
            var (center, radius, stroke, density) = GetGeometry(info);

            DrawDisc(canvas, center, radius - (stroke * DiscInsetRate), density);

            using var track = new SKPaint();
            track.IsAntialias = true;
            track.Style = SKPaintStyle.Stroke;
            track.StrokeWidth = stroke;
            track.StrokeCap = SKStrokeCap.Round;
            track.Color = TrackColor.ToSKColor();
            canvas.DrawArc(center, radius, StartAngle, SweepAngle, false, track);

            var ratio = Ratio();
            var start = IsActive ? ArcStartColor.ToSKColor() : SKColors.Gray;
            var end = IsActive ? ArcEndColor.ToSKColor() : SKColors.Gray;
            DrawValueArc(canvas, center, radius, stroke, density, ratio, start, end);
            DrawKnob(canvas, center, radius, stroke, density, ratio, end);
        }
    }

    private (SKPoint Center, float Radius, float Stroke, float Density) GetGeometry(SKImageInfo info)
    {
        var density = GetDensity(info);
        var stroke = ArcStrokeWidth * density;
        var radius = (MathF.Min(info.Width, info.Height) / 2f) - (stroke * ArcInsetRate);
        return (new SKPoint(info.Width / 2f, info.Height / 2f), radius, stroke, density);
    }

    private float Ratio()
    {
        var range = Maximum - Minimum;
        return range > 0 ? (float)Math.Clamp((Value - Minimum) / range, 0d, 1d) : 0f;
    }

    private static void DrawDisc(SKCanvas canvas, SKPoint center, float radius, float density)
    {
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(center.X, center.Y - radius),
            new SKPoint(center.X, center.Y + radius),
            [DiscTopColor, DiscBottomColor],
            [0f, 1f],
            SKShaderTileMode.Clamp);
        using var fill = new SKPaint();
        fill.IsAntialias = true;
        fill.Shader = shader;
        canvas.DrawCircle(center, radius, fill);

        using var line = new SKPaint();
        line.IsAntialias = true;
        line.Style = SKPaintStyle.Stroke;
        line.StrokeWidth = DiscLineWidth * density;
        line.Color = DiscLineColor;
        canvas.DrawCircle(center, radius, line);
    }

    private static void DrawValueArc(SKCanvas canvas, SKPoint center, float radius, float stroke, float density, float ratio, SKColor start, SKColor end)
    {
        if (ratio > 0f)
        {
            var sweep = SweepAngle * ratio;
            using var shader = AngleHelper.CreateArcGradient(center, StartAngle, sweep, [start, end]);

            using var glow = new SKPaint();
            glow.IsAntialias = true;
            glow.Style = SKPaintStyle.Stroke;
            glow.StrokeWidth = stroke * GlowWidthRate;
            glow.StrokeCap = SKStrokeCap.Round;
            glow.Shader = shader;
            glow.Color = SKColors.White.WithAlpha(GlowAlpha);
            glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowBlur * density);
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

    private static void DrawKnob(SKCanvas canvas, SKPoint center, float radius, float stroke, float density, float ratio, SKColor color)
    {
        var knob = AngleHelper.GetPoint(center, radius, StartAngle + (SweepAngle * ratio));

        using var glow = new SKPaint();
        glow.IsAntialias = true;
        glow.Color = color.WithAlpha(KnobGlowAlpha);
        glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, KnobGlowBlur * density);
        canvas.DrawCircle(knob, stroke * KnobGlowRate, glow);

        using var ring = new SKPaint();
        ring.IsAntialias = true;
        ring.Color = SKColors.White;
        canvas.DrawCircle(knob, stroke * KnobRingRate, ring);

        using var dot = new SKPaint();
        dot.IsAntialias = true;
        dot.Color = color;
        canvas.DrawCircle(knob, stroke * KnobDotRate, dot);
    }

    //--------------------------------------------------------------------------------
    // Touch
    //--------------------------------------------------------------------------------

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                dragging = IsOnRing(e.Location);
                UpdateValue(e.Location);
                break;
            case SKTouchAction.Moved:
                UpdateValue(e.Location);
                break;
            case SKTouchAction.Released:
            case SKTouchAction.Cancelled:
            case SKTouchAction.Exited:
                dragging = false;
                break;
        }

        e.Handled = dragging;
    }

    private bool IsOnRing(SKPoint location)
    {
        var (center, radius, stroke, _) = GetGeometry(new SKImageInfo(CanvasSize.Width > 0 ? (int)CanvasSize.Width : 1, CanvasSize.Height > 0 ? (int)CanvasSize.Height : 1));
        var distance = SKPoint.Distance(location, center);
        return (distance > radius - (stroke * TouchRangeRate)) && (distance < radius + (stroke * TouchRangeRate));
    }

    private void UpdateValue(SKPoint location)
    {
        if (dragging)
        {
            var center = new SKPoint(CanvasSize.Width / 2f, CanvasSize.Height / 2f);
            var angle = MathF.Atan2(location.Y - center.Y, location.X - center.X) * 180f / MathF.PI;
            var relative = (((angle - StartAngle) % 360f) + 360f) % 360f;
            if (relative > SweepAngle)
            {
                relative = relative < SweepAngle + ((360f - SweepAngle) / 2f) ? SweepAngle : 0f;
            }

            var value = Minimum + ((Maximum - Minimum) * relative / SweepAngle);
            if (Step > 0)
            {
                value = Minimum + (Math.Round((value - Minimum) / Step) * Step);
            }

            Value = Math.Clamp(value, Minimum, Maximum);
        }
    }
}

//--------------------------------------------------------------------------------
// Icon
//--------------------------------------------------------------------------------
public sealed class HomeGlowIcon : HomeControl
{
    private const float GlowBlur = 9f;

    private const byte GlowAlpha = 200;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty GlyphProperty = BindableProperty.Create(
        nameof(Glyph),
        typeof(string),
        typeof(HomeGlowIcon),
        string.Empty,
        propertyChanged: Invalidate);

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(HomeGlowIcon),
        Colors.White,
        propertyChanged: Invalidate);

    public static readonly BindableProperty IconSizeProperty = BindableProperty.Create(
        nameof(IconSize),
        typeof(double),
        typeof(HomeGlowIcon),
        40d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty IsGlowingProperty = BindableProperty.Create(
        nameof(IsGlowing),
        typeof(bool),
        typeof(HomeGlowIcon),
        false,
        propertyChanged: Invalidate);

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public bool IsGlowing
    {
        get => (bool)GetValue(IsGlowingProperty);
        set => SetValue(IsGlowingProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0) && !String.IsNullOrEmpty(Glyph))
        {
            var density = GetDensity(info);
            using var font = new SKFont(SocialFonts.MaterialIcons, (float)IconSize * density);
            var bounds = font.MeasureText(Glyph, out var rect);
            var x = (info.Width - bounds) / 2f;
            var y = (info.Height / 2f) - rect.MidY;
            var color = Color.ToSKColor();

            if (IsGlowing)
            {
                using var glow = new SKPaint();
                glow.IsAntialias = true;
                glow.Color = color.WithAlpha(GlowAlpha);
                glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowBlur * density);
                canvas.DrawText(Glyph, x, y, SKTextAlign.Left, font, glow);
            }

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Color = color;
            canvas.DrawText(Glyph, x, y, SKTextAlign.Left, font, paint);
        }
    }
}

//--------------------------------------------------------------------------------
// Battery
//--------------------------------------------------------------------------------
public sealed class HomeBatteryGauge : HomeControl
{
    private const float BodyMargin = 6f;

    private const float BodyRadius = 6f;

    private const float FillInset = 3f;

    private const float TerminalWidth = 4f;

    private const float TerminalHeightRate = 0.4f;

    private const float TerminalGap = 1f;

    private const float TerminalRadius = 1f;

    private const float LineWidth = 2f;

    private const float GlowBlur = 5f;

    private const byte GlowAlpha = 150;

    private const byte FillBottomAlpha = 170;

    private const byte LineAlpha = 220;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty LevelProperty = BindableProperty.Create(
        nameof(Level),
        typeof(double),
        typeof(HomeBatteryGauge),
        0d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty FillColorProperty = BindableProperty.Create(
        nameof(FillColor),
        typeof(Color),
        typeof(HomeBatteryGauge),
        Colors.LimeGreen,
        propertyChanged: Invalidate);

    public double Level
    {
        get => (double)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public Color FillColor
    {
        get => (Color)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            var density = GetDensity(info);
            var margin = BodyMargin * density;
            var terminal = TerminalWidth * density;
            var body = new SKRect(margin, margin, info.Width - margin - terminal, info.Height - margin);
            var radius = BodyRadius * density;
            var color = FillColor.ToSKColor();
            var level = (float)Math.Clamp(Level, 0d, 1d);

            var inset = FillInset * density;
            var inner = new SKRect(body.Left + inset, body.Top + inset, body.Right - inset, body.Bottom - inset);
            if (level > 0f)
            {
                var fill = new SKRect(inner.Left, inner.Top, inner.Left + (inner.Width * level), inner.Bottom);

                using var glow = new SKPaint();
                glow.IsAntialias = true;
                glow.Color = color.WithAlpha(GlowAlpha);
                glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowBlur * density);
                canvas.DrawRoundRect(fill, radius / 2f, radius / 2f, glow);

                using var shader = SKShader.CreateLinearGradient(
                    new SKPoint(0f, fill.Top),
                    new SKPoint(0f, fill.Bottom),
                    [color.WithAlpha(255), color.WithAlpha(FillBottomAlpha)],
                    [0f, 1f],
                    SKShaderTileMode.Clamp);
                using var paint = new SKPaint();
                paint.IsAntialias = true;
                paint.Shader = shader;
                canvas.DrawRoundRect(fill, radius / 2f, radius / 2f, paint);
            }

            using var line = new SKPaint();
            line.IsAntialias = true;
            line.Style = SKPaintStyle.Stroke;
            line.StrokeWidth = LineWidth * density;
            line.Color = color.WithAlpha(LineAlpha);
            canvas.DrawRoundRect(body, radius, radius, line);

            using var cap = new SKPaint();
            cap.IsAntialias = true;
            cap.Color = color.WithAlpha(LineAlpha);
            var capHeight = body.Height * TerminalHeightRate;
            canvas.DrawRoundRect(new SKRect(body.Right + (TerminalGap * density), body.MidY - (capHeight / 2f), body.Right + terminal, body.MidY + (capHeight / 2f)), TerminalRadius * density, TerminalRadius * density, cap);
        }
    }
}
