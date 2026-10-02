namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

// 円形の文字盤 (60 本の目盛り・溝・光る円弧・先端のつまみ)。Progress は 0〜1 で 12 時から時計回り。
// IsAlert の間は輪が AlertColor で脈打つ
public sealed class TimerDial : SKCanvasView
{
    private const string AlertAnimationName = "TimerDialAlert";

    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
        nameof(Progress),
        typeof(double),
        typeof(TimerDial),
        0d,
        propertyChanged: OnVisualChanged);

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public static readonly BindableProperty ArcStartColorProperty = BindableProperty.Create(
        nameof(ArcStartColor),
        typeof(Color),
        typeof(TimerDial),
        Colors.Cyan,
        propertyChanged: OnVisualChanged);

    public Color ArcStartColor
    {
        get => (Color)GetValue(ArcStartColorProperty);
        set => SetValue(ArcStartColorProperty, value);
    }

    public static readonly BindableProperty ArcEndColorProperty = BindableProperty.Create(
        nameof(ArcEndColor),
        typeof(Color),
        typeof(TimerDial),
        Colors.DodgerBlue,
        propertyChanged: OnVisualChanged);

    public Color ArcEndColor
    {
        get => (Color)GetValue(ArcEndColorProperty);
        set => SetValue(ArcEndColorProperty, value);
    }

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(TimerDial),
        Color.FromRgba(255, 255, 255, 20),
        propertyChanged: OnVisualChanged);

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public static readonly BindableProperty TickColorProperty = BindableProperty.Create(
        nameof(TickColor),
        typeof(Color),
        typeof(TimerDial),
        Color.FromRgba(255, 255, 255, 110),
        propertyChanged: OnVisualChanged);

    public Color TickColor
    {
        get => (Color)GetValue(TickColorProperty);
        set => SetValue(TickColorProperty, value);
    }

    public static readonly BindableProperty AlertColorProperty = BindableProperty.Create(
        nameof(AlertColor),
        typeof(Color),
        typeof(TimerDial),
        Colors.OrangeRed,
        propertyChanged: OnVisualChanged);

    public Color AlertColor
    {
        get => (Color)GetValue(AlertColorProperty);
        set => SetValue(AlertColorProperty, value);
    }

    public static readonly BindableProperty IsAlertProperty = BindableProperty.Create(
        nameof(IsAlert),
        typeof(bool),
        typeof(TimerDial),
        false,
        propertyChanged: OnAlertChanged);

    public bool IsAlert
    {
        get => (bool)GetValue(IsAlertProperty);
        set => SetValue(IsAlertProperty, value);
    }

    // 脈打ちの位相 (0〜1)
    private float alertPhase;

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((TimerDial)bindable).InvalidateSurface();

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
                length: 1200,
                repeat: () => dial.IsAlert);
        }
        else
        {
            dial.AbortAnimation(AlertAnimationName);
            dial.alertPhase = 0;
            dial.InvalidateSurface();
        }
    }

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
            var radius = (MathF.Min(info.Width, info.Height) / 2f) - (6 * density);
            var arcRadius = radius - (20 * density);
            var stroke = 12 * density;

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

    // 5 本ごとに長く明るい目盛り
    private void DrawTicks(SKCanvas canvas, SKPoint center, float radius, float density)
    {
        var color = TickColor.ToSKColor();
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeCap = SKStrokeCap.Round;
        for (var i = 0; i < 60; i++)
        {
            var major = (i % 5) == 0;
            paint.StrokeWidth = (major ? 2.5f : 1.2f) * density;
            paint.Color = major ? color : color.WithAlpha((byte)(color.Alpha / 2));
            var length = (major ? 11f : 5f) * density;
            var angle = ((i * 6f) - 90f) * MathF.PI / 180f;
            var cos = MathF.Cos(angle);
            var sin = MathF.Sin(angle);
            canvas.DrawLine(
                center.X + (cos * (radius - length)),
                center.Y + (sin * (radius - length)),
                center.X + (cos * radius),
                center.Y + (sin * radius),
                paint);
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

    // グラデーションの円弧と、ぼかした同じ円弧の光。先端に光るつまみ
    private void DrawArc(SKCanvas canvas, SKPoint center, float radius, float stroke, float density)
    {
        var progress = (float)Math.Clamp(Progress, 0d, 1d);
        if (progress > 0f)
        {
            var sweep = 360f * progress;
            var rect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);

            // SKShader.CreateSweepGradient は 3 時の方向が 0° なので、12 時から始まるように -90° 回す
            using var baseShader = SKShader.CreateSweepGradient(
                center,
                [ArcStartColor.ToSKColor(), ArcEndColor.ToSKColor()],
                [0f, progress],
                SKShaderTileMode.Clamp,
                0f,
                360f);
            using var shader = baseShader.WithLocalMatrix(SKMatrix.CreateRotationDegrees(-90f, center.X, center.Y));
            using var builder = new SKPathBuilder();
            builder.AddArc(rect, -90f, sweep);
            using var path = builder.Detach();

            using var glow = new SKPaint();
            glow.IsAntialias = true;
            glow.Style = SKPaintStyle.Stroke;
            glow.StrokeWidth = stroke * 1.8f;
            glow.StrokeCap = SKStrokeCap.Round;
            glow.Shader = shader;
            glow.Color = SKColors.White.WithAlpha(140);
            glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 9 * density);
            canvas.DrawPath(path, glow);

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = stroke;
            paint.StrokeCap = SKStrokeCap.Round;
            paint.Shader = shader;
            canvas.DrawPath(path, paint);

            var end = (sweep - 90f) * MathF.PI / 180f;
            var knob = new SKPoint(center.X + (MathF.Cos(end) * radius), center.Y + (MathF.Sin(end) * radius));
            using var knobGlow = new SKPaint();
            knobGlow.IsAntialias = true;
            knobGlow.Color = ArcEndColor.ToSKColor();
            knobGlow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6 * density);
            canvas.DrawCircle(knob, stroke, knobGlow);

            using var knobPaint = new SKPaint();
            knobPaint.IsAntialias = true;
            knobPaint.Color = SKColors.White;
            canvas.DrawCircle(knob, stroke * 0.42f, knobPaint);
        }
    }

    // 輪全体を AlertColor で脈打たせる
    private void DrawAlert(SKCanvas canvas, SKPoint center, float radius, float stroke, float density)
    {
        var strength = 0.5f - (0.5f * MathF.Cos(alertPhase * 2f * MathF.PI));
        var color = AlertColor.ToSKColor();

        using var glow = new SKPaint();
        glow.IsAntialias = true;
        glow.Style = SKPaintStyle.Stroke;
        glow.StrokeWidth = stroke * (1.4f + strength);
        glow.Color = color.WithAlpha((byte)(80 + (150 * strength)));
        glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, (6 + (10 * strength)) * density);
        canvas.DrawCircle(center, radius, glow);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = stroke;
        paint.Color = color.WithAlpha((byte)(160 + (95 * strength)));
        canvas.DrawCircle(center, radius, paint);
    }
}
