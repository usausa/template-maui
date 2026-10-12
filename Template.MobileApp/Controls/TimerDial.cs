namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

public sealed class TimerDial : SKCanvasView
{
    private const string AlertAnimationName = "TimerDialAlert";

    private const uint AlertLength = 1200;

    private const float StartAngle = -90f;

    private const float DialMargin = 6f;

    private const float ArcInset = 20f;

    private const float ArcStrokeWidth = 12f;

    private const int TickCount = 60;

    private const float TickAngle = 360f / TickCount;

    private const int MajorTickStep = 5;

    private const float MajorTickWidth = 2.5f;

    private const float MinorTickWidth = 1.2f;

    private const float MajorTickLength = 11f;

    private const float MinorTickLength = 5f;

    private const float GlowBlur = 9f;

    private const float KnobGlowBlur = 6f;

    private const float KnobRadiusRate = 0.42f;

    private const float AlertGlowWidthRate = 1.4f;

    private const float AlertGlowMinimumAlpha = 80f;

    private const float AlertGlowAlphaRange = 150f;

    private const float AlertGlowBlur = 6f;

    private const float AlertGlowBlurRange = 10f;

    private const float AlertMinimumAlpha = 160f;

    private const float AlertAlphaRange = 95f;

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private float alertPhase;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
        nameof(Progress),
        typeof(double),
        typeof(TimerDial),
        0d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty ArcStartColorProperty = BindableProperty.Create(
        nameof(ArcStartColor),
        typeof(Color),
        typeof(TimerDial),
        Colors.Cyan,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty ArcEndColorProperty = BindableProperty.Create(
        nameof(ArcEndColor),
        typeof(Color),
        typeof(TimerDial),
        Colors.DodgerBlue,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(TimerDial),
        Color.FromRgba(255, 255, 255, 20),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TickColorProperty = BindableProperty.Create(
        nameof(TickColor),
        typeof(Color),
        typeof(TimerDial),
        Color.FromRgba(255, 255, 255, 110),
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty AlertColorProperty = BindableProperty.Create(
        nameof(AlertColor),
        typeof(Color),
        typeof(TimerDial),
        Colors.OrangeRed,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty IsAlertProperty = BindableProperty.Create(
        nameof(IsAlert),
        typeof(bool),
        typeof(TimerDial),
        false,
        propertyChanged: OnAlertChanged);

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
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

    public Color TickColor
    {
        get => (Color)GetValue(TickColorProperty);
        set => SetValue(TickColorProperty, value);
    }

    public Color AlertColor
    {
        get => (Color)GetValue(AlertColorProperty);
        set => SetValue(AlertColorProperty, value);
    }

    public bool IsAlert
    {
        get => (bool)GetValue(IsAlertProperty);
        set => SetValue(IsAlertProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((TimerDial)bindable).InvalidateSurface();

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
            var arcRadius = radius - (ArcInset * density);
            var stroke = ArcStrokeWidth * density;

            DrawTicks(canvas, center, radius, density);
            DrawTrack(canvas, center, arcRadius, stroke);
            if (IsAlert)
            {
                DrawAlert(canvas, center, arcRadius, stroke, density);
            }
            else
            {
                DrawArc(canvas, center, arcRadius, stroke, density);
            }
        }
    }

    private void DrawTicks(SKCanvas canvas, SKPoint center, float radius, float density)
    {
        var color = TickColor.ToSKColor();
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeCap = SKStrokeCap.Round;
        for (var i = 0; i < TickCount; i++)
        {
            var major = (i % MajorTickStep) == 0;
            paint.StrokeWidth = (major ? MajorTickWidth : MinorTickWidth) * density;
            paint.Color = major ? color : color.WithAlpha((byte)(color.Alpha / 2));
            var length = (major ? MajorTickLength : MinorTickLength) * density;
            var angle = (i * TickAngle) + StartAngle;
            canvas.DrawLine(AngleHelper.GetPoint(center, radius - length, angle), AngleHelper.GetPoint(center, radius, angle), paint);
        }
    }

    private void DrawTrack(SKCanvas canvas, SKPoint center, float radius, float stroke)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = stroke;
        paint.Color = TrackColor.ToSKColor();
        canvas.DrawCircle(center, radius, paint);
    }

    private void DrawArc(SKCanvas canvas, SKPoint center, float radius, float stroke, float density)
    {
        var progress = (float)Math.Clamp(Progress, 0d, 1d);
        if (progress > 0f)
        {
            GlowRing.DrawGlowArc(canvas, center, radius, stroke, GlowBlur * density, progress, ArcStartColor.ToSKColor(), ArcEndColor.ToSKColor());

            var knob = AngleHelper.GetPoint(center, radius, (360f * progress) + StartAngle);
            using var knobGlow = new SKPaint();
            knobGlow.IsAntialias = true;
            knobGlow.Color = ArcEndColor.ToSKColor();
            knobGlow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, KnobGlowBlur * density);
            canvas.DrawCircle(knob, stroke, knobGlow);

            using var knobPaint = new SKPaint();
            knobPaint.IsAntialias = true;
            knobPaint.Color = SKColors.White;
            canvas.DrawCircle(knob, stroke * KnobRadiusRate, knobPaint);
        }
    }

    private void DrawAlert(SKCanvas canvas, SKPoint center, float radius, float stroke, float density)
    {
        var strength = 0.5f - (0.5f * MathF.Cos(alertPhase * 2f * MathF.PI));
        var color = AlertColor.ToSKColor();

        using var glow = new SKPaint();
        glow.IsAntialias = true;
        glow.Style = SKPaintStyle.Stroke;
        glow.StrokeWidth = stroke * (AlertGlowWidthRate + strength);
        glow.Color = color.WithAlpha((byte)(AlertGlowMinimumAlpha + (AlertGlowAlphaRange * strength)));
        glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, (AlertGlowBlur + (AlertGlowBlurRange * strength)) * density);
        canvas.DrawCircle(center, radius, glow);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = stroke;
        paint.Color = color.WithAlpha((byte)(AlertMinimumAlpha + (AlertAlphaRange * strength)));
        canvas.DrawCircle(center, radius, paint);
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private static void OnAlertChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var dial = (TimerDial)bindable;
        if ((bool)newValue)
        {
            dial.Animate(
                AlertAnimationName,
                x =>
                {
                    dial.alertPhase = (float)x;
                    dial.InvalidateSurface();
                },
                length: AlertLength,
                repeat: () => dial.IsAlert);
        }
        else
        {
            dial.AbortAnimation(AlertAnimationName);
            dial.alertPhase = 0;
            dial.InvalidateSurface();
        }
    }
}
