namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

//--------------------------------------------------------------------------------
// Hourly
//--------------------------------------------------------------------------------
public sealed class SeriesSegmentView : SKCanvasView
{
    private const string RippleAnimationName = "SeriesRipple";

    private const string RevealAnimationName = "SeriesReveal";

    private const int RevealLength = 80;

    private const uint AnimationRate = 16;

    private const uint RippleRate = 32;

    private const uint RippleLength = 1800;

    private const float TextSpacing = 12f;

    private const float TextOffset = 10f;

    private const float PointMargin = 8f;

    private const float LineWidth = 2f;

    private const float PointRadius = 3f;

    private const float HighlightedPointRadius = 4.5f;

    private const float HaloRadius = 8f;

    private const float RippleWidth = 1.5f;

    private const float RippleSpread = 10f;

    private const float RippleAlpha = 200f;

    private const byte HaloAlpha = 90;

    private static readonly SKTypeface BoldTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private float ripple;

    private float reveal = 1f;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(SeriesSegmentView),
        0d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty PreviousProperty = BindableProperty.Create(
        nameof(Previous),
        typeof(double?),
        typeof(SeriesSegmentView),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty NextProperty = BindableProperty.Create(
        nameof(Next),
        typeof(double?),
        typeof(SeriesSegmentView),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(SeriesSegmentView),
        0d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(SeriesSegmentView),
        1d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(SeriesSegmentView),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(SeriesSegmentView),
        14d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(SeriesSegmentView),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty LineColorProperty = BindableProperty.Create(
        nameof(LineColor),
        typeof(Color),
        typeof(SeriesSegmentView),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty FillColorProperty = BindableProperty.Create(
        nameof(FillColor),
        typeof(Color),
        typeof(SeriesSegmentView),
        Color.FromRgba(255, 255, 255, 64),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty IsHighlightedProperty = BindableProperty.Create(
        nameof(IsHighlighted),
        typeof(bool),
        typeof(SeriesSegmentView),
        false,
        propertyChanged: OnHighlightedChanged);

    public static readonly BindableProperty RevealOrderProperty = BindableProperty.Create(
        nameof(RevealOrder),
        typeof(int),
        typeof(SeriesSegmentView),
        -1);

    private static readonly BindableProperty RevealOriginProperty = BindableProperty.CreateAttached(
        "RevealOrigin",
        typeof(long),
        typeof(SeriesSegmentView),
        0L);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double? Previous
    {
        get => (double?)GetValue(PreviousProperty);
        set => SetValue(PreviousProperty, value);
    }

    public double? Next
    {
        get => (double?)GetValue(NextProperty);
        set => SetValue(NextProperty, value);
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

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Color LineColor
    {
        get => (Color)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    public Color FillColor
    {
        get => (Color)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }

    public int RevealOrder
    {
        get => (int)GetValue(RevealOrderProperty);
        set => SetValue(RevealOrderProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SeriesSegmentView()
    {
        Loaded += (_, _) =>
        {
            UpdateRipple();
            StartReveal();
        };
        Unloaded += (_, _) =>
        {
            this.AbortAnimation(RippleAnimationName);
            this.AbortAnimation(RevealAnimationName);
        };
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((SeriesSegmentView)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            var density = (float)(info.Width / Width);
            using var font = new SKFont(BoldTypeface, (float)FontSize * density);

            var top = font.Metrics.GetLineHeight() + (TextSpacing * density);
            var bottom = info.Height - (PointMargin * density);
            var center = new SKPoint(info.Width / 2f, ToY(Value, top, bottom));
            var left = new SKPoint(0, Previous is { } previous ? ToY((previous + Value) / 2, top, bottom) : center.Y);
            var right = new SKPoint(info.Width, Next is { } next ? ToY((Value + next) / 2, top, bottom) : center.Y);
            var start = Previous is null ? center : left;
            var end = Next is null ? center : right;

            canvas.Save();
            canvas.ClipRect(new SKRect(0, 0, info.Width * reveal, info.Height));
            DrawFill(canvas, start, center, end, top, info.Height);
            DrawLine(canvas, start, center, end, density);
            canvas.Restore();

            if (reveal >= 0.5f)
            {
                DrawPoint(canvas, center, density);
                DrawText(canvas, center, font, density);
            }
        }
    }

    private float ToY(double value, float top, float bottom)
    {
        var range = Maximum - Minimum;
        var ratio = range > 0 ? Math.Clamp((value - Minimum) / range, 0, 1) : 0.5;
        return bottom - (float)(ratio * (bottom - top));
    }

    private void DrawFill(SKCanvas canvas, SKPoint start, SKPoint center, SKPoint end, float top, float height)
    {
        using var builder = new SKPathBuilder();
        builder.MoveTo(start);
        builder.LineTo(center);
        builder.LineTo(end);
        builder.LineTo(end.X, height);
        builder.LineTo(start.X, height);
        builder.Close();
        using var path = builder.Detach();

        var color = FillColor.ToSKColor();
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, top),
            new SKPoint(0, height),
            [color, color.WithAlpha(0)],
            SKShaderTileMode.Clamp);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Shader = shader;
        canvas.DrawPath(path, paint);
    }

    private void DrawLine(SKCanvas canvas, SKPoint start, SKPoint center, SKPoint end, float density)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = LineWidth * density;
        paint.Color = LineColor.ToSKColor();
        canvas.DrawLine(start, center, paint);
        canvas.DrawLine(center, end, paint);
    }

    private void DrawPoint(SKCanvas canvas, SKPoint center, float density)
    {
        var color = LineColor.ToSKColor();
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        if (IsHighlighted)
        {
            paint.Color = color.WithAlpha(HaloAlpha);
            canvas.DrawCircle(center, HaloRadius * density, paint);

            if (ripple > 0f)
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = RippleWidth * density;
                paint.Color = color.WithAlpha((byte)(RippleAlpha * (1f - ripple)));
                canvas.DrawCircle(center, (HaloRadius + (RippleSpread * ripple)) * density, paint);
                paint.Style = SKPaintStyle.Fill;
            }
        }

        paint.Color = color;
        canvas.DrawCircle(center, (IsHighlighted ? HighlightedPointRadius : PointRadius) * density, paint);
    }

    private void DrawText(SKCanvas canvas, SKPoint center, SKFont font, float density)
    {
        if (!String.IsNullOrEmpty(Text))
        {
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Color = TextColor.ToSKColor();
            canvas.DrawText(Text, center.X, font.Metrics.GetBottomBaseline(center.Y - (TextOffset * density)), SKTextAlign.Center, font, paint);
        }
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private static void OnHighlightedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SeriesSegmentView)bindable;
        control.UpdateRipple();
        control.InvalidateSurface();
    }

    private void UpdateRipple()
    {
        this.AbortAnimation(RippleAnimationName);
        ripple = 0f;
        if (IsHighlighted && IsLoaded)
        {
            this.Animate(
                RippleAnimationName,
                v =>
                {
                    ripple = (float)v;
                    InvalidateSurface();
                },
                0d,
                1d,
                RippleRate,
                RippleLength,
                Easing.CubicOut,
                repeat: () => IsHighlighted);
        }
    }

    private void StartReveal()
    {
        this.AbortAnimation(RevealAnimationName);
        reveal = 1f;

        var owner = FindItemsView();
        if ((RevealOrder >= 0) && (owner is not null))
        {
            var now = Environment.TickCount64;
            var origin = (long)owner.GetValue(RevealOriginProperty);
            if (origin == 0)
            {
                origin = now;
                owner.SetValue(RevealOriginProperty, origin);
            }

            var wait = origin + ((long)RevealOrder * RevealLength) - now;
            if (wait + RevealLength > 0)
            {
                var length = Math.Max(wait, 0) + RevealLength;
                reveal = (float)Math.Clamp(-wait / (double)RevealLength, 0, 1);
                this.Animate(
                    RevealAnimationName,
                    v =>
                    {
                        reveal = (float)Math.Clamp(((v * length) - wait) / RevealLength, 0, 1);
                        InvalidateSurface();
                    },
                    0d,
                    1d,
                    AnimationRate,
                    (uint)length,
                    Easing.Linear,
                    (_, _) =>
                    {
                        reveal = 1f;
                        InvalidateSurface();
                    });
            }
        }
    }

    private ItemsView? FindItemsView()
    {
        for (var element = Parent; element is not null; element = element.Parent)
        {
            if (element is ItemsView view)
            {
                return view;
            }
        }

        return null;
    }
}

//--------------------------------------------------------------------------------
// Level
//--------------------------------------------------------------------------------
public sealed class SegmentBar : SKCanvasView
{
    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(int),
        typeof(SegmentBar),
        0,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(int),
        typeof(SegmentBar),
        5,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty ActiveColorProperty = BindableProperty.Create(
        nameof(ActiveColor),
        typeof(Color),
        typeof(SegmentBar),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty InactiveColorProperty = BindableProperty.Create(
        nameof(InactiveColor),
        typeof(Color),
        typeof(SegmentBar),
        Color.FromRgba(255, 255, 255, 50),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty SpacingProperty = BindableProperty.Create(
        nameof(Spacing),
        typeof(double),
        typeof(SegmentBar),
        3d,
        propertyChanged: OnVisualChanged);

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public Color ActiveColor
    {
        get => (Color)GetValue(ActiveColorProperty);
        set => SetValue(ActiveColorProperty, value);
    }

    public Color InactiveColor
    {
        get => (Color)GetValue(InactiveColorProperty);
        set => SetValue(InactiveColorProperty, value);
    }

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((SegmentBar)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0) && (Maximum > 0))
        {
            var density = (float)(info.Width / Width);
            var spacing = (float)Spacing * density;
            var width = (info.Width - (spacing * (Maximum - 1))) / Maximum;
            var radius = info.Height / 2f;

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            for (var i = 0; i < Maximum; i++)
            {
                paint.Color = (i < Value ? ActiveColor : InactiveColor).ToSKColor();
                var left = i * (width + spacing);
                canvas.DrawRoundRect(new SKRect(left, 0, left + width, info.Height), radius, radius, paint);
            }
        }
    }
}

//--------------------------------------------------------------------------------
// Wind
//--------------------------------------------------------------------------------
public sealed class WeatherCompassDial : SKCanvasView
{
    private const float DialMargin = 4f;

    private const float LabelInset = 20f;

    private const float ArrowInset = 12f;

    private const int TickStep = 5;

    private const int MajorTickStep = 30;

    private const float MajorTickWidth = 2f;

    private const float MinorTickWidth = 1f;

    private const float MajorTickLength = 7f;

    private const float MinorTickLength = 4f;

    private const float ArrowGapRate = 0.45f;

    private const float ArrowStrokeWidth = 2.5f;

    private const float TailInset = 4f;

    private const float TailRadius = 3.5f;

    private const float ShaftInset = 8f;

    private const float HeadLength = 12f;

    private const float HeadHalfWidth = 6f;

    private static readonly SKTypeface BoldTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

    private static readonly string[] Labels = ["N", "E", "S", "W"];

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty DirectionProperty = BindableProperty.Create(
        nameof(Direction),
        typeof(double),
        typeof(WeatherCompassDial),
        0d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TickColorProperty = BindableProperty.Create(
        nameof(TickColor),
        typeof(Color),
        typeof(WeatherCompassDial),
        Color.FromRgba(255, 255, 255, 110),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(
        nameof(LabelColor),
        typeof(Color),
        typeof(WeatherCompassDial),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty LabelFontSizeProperty = BindableProperty.Create(
        nameof(LabelFontSize),
        typeof(double),
        typeof(WeatherCompassDial),
        12d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty NorthColorProperty = BindableProperty.Create(
        nameof(NorthColor),
        typeof(Color),
        typeof(WeatherCompassDial),
        Color.FromArgb("#FF8A80"),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty ArrowColorProperty = BindableProperty.Create(
        nameof(ArrowColor),
        typeof(Color),
        typeof(WeatherCompassDial),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public double Direction
    {
        get => (double)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public Color TickColor
    {
        get => (Color)GetValue(TickColorProperty);
        set => SetValue(TickColorProperty, value);
    }

    public Color LabelColor
    {
        get => (Color)GetValue(LabelColorProperty);
        set => SetValue(LabelColorProperty, value);
    }

    public double LabelFontSize
    {
        get => (double)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public Color NorthColor
    {
        get => (Color)GetValue(NorthColorProperty);
        set => SetValue(NorthColorProperty, value);
    }

    public Color ArrowColor
    {
        get => (Color)GetValue(ArrowColorProperty);
        set => SetValue(ArrowColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((WeatherCompassDial)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            var density = (float)(info.Width / Width);
            var center = new SKPoint(info.Width / 2f, info.Height / 2f);
            var radius = (MathF.Min(info.Width, info.Height) / 2f) - (DialMargin * density);

            DrawTicks(canvas, center, radius, density);
            DrawLabels(canvas, center, radius - (LabelInset * density), density);
            DrawArrow(canvas, center, radius - (ArrowInset * density), density);
        }
    }

    private void DrawTicks(SKCanvas canvas, SKPoint center, float radius, float density)
    {
        var color = TickColor.ToSKColor();
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeCap = SKStrokeCap.Round;
        for (var degree = 0; degree < 360; degree += TickStep)
        {
            var major = (degree % MajorTickStep) == 0;
            paint.StrokeWidth = (major ? MajorTickWidth : MinorTickWidth) * density;
            paint.Color = major ? color : color.WithAlpha((byte)(color.Alpha / 2));
            canvas.DrawLine(PointAt(center, radius, degree), PointAt(center, radius - ((major ? MajorTickLength : MinorTickLength) * density), degree), paint);
        }
    }

    private void DrawLabels(SKCanvas canvas, SKPoint center, float radius, float density)
    {
        using var font = new SKFont(BoldTypeface, (float)LabelFontSize * density);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        for (var i = 0; i < Labels.Length; i++)
        {
            var point = PointAt(center, radius, i * 90);
            paint.Color = (i == 0 ? NorthColor : LabelColor).ToSKColor();
            canvas.DrawText(Labels[i], point.X, font.Metrics.GetCenterBaseline(point.Y), SKTextAlign.Center, font, paint);
        }
    }

    private void DrawArrow(SKCanvas canvas, SKPoint center, float radius, float density)
    {
        var from = Direction;
        var to = Direction + 180;
        var gap = radius * ArrowGapRate;

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = ArrowStrokeWidth * density;
        paint.StrokeCap = SKStrokeCap.Round;
        paint.Color = ArrowColor.ToSKColor();
        var tail = PointAt(center, radius - (TailInset * density), from);
        canvas.DrawLine(tail, PointAt(center, gap, from), paint);
        var head = PointAt(center, radius, to);
        canvas.DrawLine(PointAt(center, gap, to), PointAt(center, radius - (ShaftInset * density), to), paint);

        paint.Style = SKPaintStyle.Fill;
        canvas.DrawCircle(tail, TailRadius * density, paint);

        var back = PointAt(center, radius - (HeadLength * density), to);
        var offset = AngleHelper.GetPoint(SKPoint.Empty, HeadHalfWidth * density, (float)to);
        using var builder = new SKPathBuilder();
        builder.MoveTo(head);
        builder.LineTo(back.X + offset.X, back.Y + offset.Y);
        builder.LineTo(back.X - offset.X, back.Y - offset.Y);
        builder.Close();
        using var path = builder.Detach();
        canvas.DrawPath(path, paint);
    }

    private static SKPoint PointAt(SKPoint center, float radius, double degree) =>
        AngleHelper.GetPoint(center, radius, AngleHelper.ToSkiaAngle((float)degree));
}

//--------------------------------------------------------------------------------
// Sun
//--------------------------------------------------------------------------------
public sealed class WeatherSunArcView : SKCanvasView
{
    private const string RevealAnimationName = "SunArcReveal";

    private const uint AnimationRate = 16;

    private const uint RevealLength = 900;

    private const float SunRadius = 7f;

    private const float SunMargin = 6f;

    private const float PathStrokeWidth = 1.5f;

    private const float PathDash = 4f;

    private const float PassedStrokeWidth = 3f;

    private const float HorizonStrokeWidth = 1f;

    private const float GlowBlur = 5f;

    private const float GlowRadiusRate = 1.4f;

    private const byte FillAlpha = 110;

    private const byte DayGlowAlpha = 150;

    private const byte NightGlowAlpha = 60;

    private const byte NightSunAlpha = 110;

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private float reveal = 1f;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty SunriseProperty = BindableProperty.Create(
        nameof(Sunrise),
        typeof(DateTime),
        typeof(WeatherSunArcView),
        default(DateTime),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty SunsetProperty = BindableProperty.Create(
        nameof(Sunset),
        typeof(DateTime),
        typeof(WeatherSunArcView),
        default(DateTime),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty NowProperty = BindableProperty.Create(
        nameof(Now),
        typeof(DateTime),
        typeof(WeatherSunArcView),
        default(DateTime),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty PathColorProperty = BindableProperty.Create(
        nameof(PathColor),
        typeof(Color),
        typeof(WeatherSunArcView),
        Color.FromRgba(255, 255, 255, 90),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty SunColorProperty = BindableProperty.Create(
        nameof(SunColor),
        typeof(Color),
        typeof(WeatherSunArcView),
        Color.FromArgb("#FFD54F"),
        propertyChanged: OnVisualChanged);

    public DateTime Sunrise
    {
        get => (DateTime)GetValue(SunriseProperty);
        set => SetValue(SunriseProperty, value);
    }

    public DateTime Sunset
    {
        get => (DateTime)GetValue(SunsetProperty);
        set => SetValue(SunsetProperty, value);
    }

    public DateTime Now
    {
        get => (DateTime)GetValue(NowProperty);
        set => SetValue(NowProperty, value);
    }

    public Color PathColor
    {
        get => (Color)GetValue(PathColorProperty);
        set => SetValue(PathColorProperty, value);
    }

    public Color SunColor
    {
        get => (Color)GetValue(SunColorProperty);
        set => SetValue(SunColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public WeatherSunArcView()
    {
        Loaded += (_, _) => StartReveal();
        Unloaded += (_, _) => this.AbortAnimation(RevealAnimationName);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((WeatherSunArcView)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0) && (Sunset > Sunrise))
        {
            var density = (float)(info.Width / Width);
            var sun = SunRadius * density;
            var margin = sun + (SunMargin * density);
            var horizon = info.Height - margin;

            var oval = new SKRect(margin, margin, info.Width - margin, (horizon * 2) - margin);
            var progress = (float)Math.Clamp((Now - Sunrise).TotalMinutes / (Sunset - Sunrise).TotalMinutes, 0, 1) * reveal;
            var isDay = (Now > Sunrise) && (Now < Sunset);
            var sunColor = SunColor.ToSKColor();

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = PathStrokeWidth * density;
            paint.Color = PathColor.ToSKColor();
            paint.PathEffect = SKPathEffect.CreateDash([PathDash * density, PathDash * density], 0);
            canvas.DrawArc(oval, 180, 180, false, paint);
            paint.PathEffect = null;

            if (progress > 0)
            {
                using var fill = new SKPaint();
                fill.IsAntialias = true;
                using var shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, margin),
                    new SKPoint(0, horizon),
                    [sunColor.WithAlpha(FillAlpha), sunColor.WithAlpha(0)],
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

                paint.StrokeWidth = PassedStrokeWidth * density;
                paint.StrokeCap = SKStrokeCap.Round;
                paint.Color = sunColor;
                canvas.DrawArc(oval, 180, 180 * progress, false, paint);
            }

            paint.StrokeWidth = HorizonStrokeWidth * density;
            paint.Color = PathColor.ToSKColor();
            canvas.DrawLine(0, horizon, info.Width, horizon, paint);

            var position = PointAt(oval, 180 + (180 * progress));
            using var glow = new SKPaint();
            glow.IsAntialias = true;
            glow.Color = sunColor.WithAlpha(isDay ? DayGlowAlpha : NightGlowAlpha);
            glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowBlur * density);
            canvas.DrawCircle(position, sun * GlowRadiusRate, glow);

            paint.Style = SKPaintStyle.Fill;
            paint.Color = isDay ? sunColor : sunColor.WithAlpha(NightSunAlpha);
            canvas.DrawCircle(position, sun, paint);
        }
    }

    private static SKPoint PointAt(SKRect oval, float angle)
    {
        var radian = angle * MathF.PI / 180f;
        return new SKPoint(oval.MidX + (MathF.Cos(radian) * oval.Width / 2), oval.MidY + (MathF.Sin(radian) * oval.Height / 2));
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private void StartReveal()
    {
        this.AbortAnimation(RevealAnimationName);
        reveal = 0f;
        this.Animate(
            RevealAnimationName,
            v =>
            {
                reveal = (float)v;
                InvalidateSurface();
            },
            0d,
            1d,
            AnimationRate,
            RevealLength,
            Easing.CubicOut);
    }
}

//--------------------------------------------------------------------------------
// Moon
//--------------------------------------------------------------------------------
public sealed class WeatherMoonPhaseView : SKCanvasView
{
    private const double SynodicMonth = 29.530588853;

    private const float MoonMargin = 8;

    private const float GlowMinimumAlpha = 30;

    private const float GlowAlphaRange = 70;

    private const float GlowBlur = 6;

    private const float GlowSpread = 2;

    private const float HighlightOffsetRate = 0.3f;

    private const float HighlightRadiusRate = 1.6f;

    private const byte LitEdgeAlpha = 210;

    private const byte SeaAlpha = 22;

    private static readonly (float X, float Y, float Size)[] Seas = [(-0.28f, -0.22f, 0.22f), (0.25f, 0.05f, 0.28f), (-0.05f, 0.42f, 0.16f)];

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty AgeProperty = BindableProperty.Create(
        nameof(Age),
        typeof(double),
        typeof(WeatherMoonPhaseView),
        0d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty LightColorProperty = BindableProperty.Create(
        nameof(LightColor),
        typeof(Color),
        typeof(WeatherMoonPhaseView),
        Color.FromArgb("#FFF4C7"),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty DarkColorProperty = BindableProperty.Create(
        nameof(DarkColor),
        typeof(Color),
        typeof(WeatherMoonPhaseView),
        Color.FromRgba(255, 255, 255, 36),
        propertyChanged: OnVisualChanged);

    public double Age
    {
        get => (double)GetValue(AgeProperty);
        set => SetValue(AgeProperty, value);
    }

    public Color LightColor
    {
        get => (Color)GetValue(LightColorProperty);
        set => SetValue(LightColorProperty, value);
    }

    public Color DarkColor
    {
        get => (Color)GetValue(DarkColorProperty);
        set => SetValue(DarkColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((WeatherMoonPhaseView)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            var density = (float)(info.Width / Width);
            var center = new SKPoint(info.Width / 2f, info.Height / 2f);
            var radius = (MathF.Min(info.Width, info.Height) / 2f) - (MoonMargin * density);
            var phase = (float)((((Age % SynodicMonth) + SynodicMonth) % SynodicMonth) / SynodicMonth);
            var light = LightColor.ToSKColor();

            using var glow = new SKPaint();
            glow.IsAntialias = true;
            glow.Color = light.WithAlpha((byte)(GlowMinimumAlpha + (GlowAlphaRange * (1 - MathF.Cos(phase * 2 * MathF.PI)) / 2)));
            glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowBlur * density);
            canvas.DrawCircle(center, radius + (GlowSpread * density), glow);

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Color = DarkColor.ToSKColor();
            canvas.DrawCircle(center, radius, paint);

            using var path = CreateLitPath(center, radius, phase);
            using var shader = SKShader.CreateRadialGradient(
                new SKPoint(center.X - (radius * HighlightOffsetRate), center.Y - (radius * HighlightOffsetRate)),
                radius * HighlightRadiusRate,
                [light, light.WithAlpha(LitEdgeAlpha)],
                SKShaderTileMode.Clamp);
            paint.Shader = shader;
            canvas.DrawPath(path, paint);
            paint.Shader = null;

            canvas.Save();
            canvas.ClipPath(path, antialias: true);
            paint.Color = SKColors.Black.WithAlpha(SeaAlpha);
            foreach (var (x, y, size) in Seas)
            {
                canvas.DrawCircle(center.X + (radius * x), center.Y + (radius * y), radius * size, paint);
            }

            canvas.Restore();
        }
    }

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

        var towardRight = waxing == crescent;
        builder.ArcTo(terminator, 90, towardRight ? -180 : 180, false);
        builder.Close();
        return builder.Detach();
    }
}
