namespace Template.MobileApp.Controls;

using System.Collections.Specialized;

#pragma warning disable CA1001
public sealed class RadarScreen : GraphicsView, IDrawable
{
    private const float StepSpeed = 2f;

    private const float TargetFadeDuration = 2500f;

    private const float GlowAlpha = 0.2f;

    private const int RingCount = 3;

    private const int CrossStep = 45;

    private const int MemoryStep = 5;

    private const int MemoryLongStep = 10;

    private const float CompassFontSize = 16f;

    private const float CompassInset = 18f;

    private const float CompassBaseline = 6f;

    private const float TargetHaloAlpha = 0.25f;

    private const float TargetHaloRadius = 10f;

    private const float TargetRadius = 5f;

    private const int SweepStep = 2;

    private const float ScanLineAlpha = 0.18f;

    private const float ScanLineWidth = 1f;

    private const float ScanLineSpacing = 4f;

    private const float VignetteStart = 0.7f;

    private const float VignetteAlpha = 0.55f;

    private const double VignetteRadius = 0.72;

    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1000d / 60);

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private readonly Dictionary<RadarTarget, long> targetPassTimes = [];

    private CancellationTokenSource? cts;

    private float currentAngle;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty BorderMarginProperty = BindableProperty.Create(
        nameof(BorderMargin),
        typeof(float),
        typeof(RadarScreen),
        10f);

    public static readonly BindableProperty MemoryColorProperty = BindableProperty.Create(
        nameof(MemoryColor),
        typeof(Color),
        typeof(RadarScreen),
        Colors.Green);

    public static readonly BindableProperty MemoryLineWidthProperty = BindableProperty.Create(
        nameof(MemoryLineWidth),
        typeof(float),
        typeof(RadarScreen),
        3f);

    public static readonly BindableProperty MemoryLengthShortProperty = BindableProperty.Create(
        nameof(MemoryLengthShort),
        typeof(float),
        typeof(RadarScreen),
        8f);

    public static readonly BindableProperty MemoryLengthLongProperty = BindableProperty.Create(
        nameof(MemoryLengthLong),
        typeof(float),
        typeof(RadarScreen),
        16f);

    public static readonly BindableProperty SweepAngleProperty = BindableProperty.Create(
        nameof(SweepAngle),
        typeof(float),
        typeof(RadarScreen),
        60f);

    public static readonly BindableProperty SweepArcAlphaProperty = BindableProperty.Create(
        nameof(SweepArcAlpha),
        typeof(byte),
        typeof(RadarScreen),
        (byte)12);

    public static readonly BindableProperty SweepColorProperty = BindableProperty.Create(
        nameof(SweepColor),
        typeof(Color),
        typeof(RadarScreen),
        Colors.Lime);

    public static readonly BindableProperty SweepLineWidthProperty = BindableProperty.Create(
        nameof(SweepLineWidth),
        typeof(float),
        typeof(RadarScreen),
        3f);

    public static readonly BindableProperty ScreenColorProperty = BindableProperty.Create(
        nameof(ScreenColor),
        typeof(Color),
        typeof(RadarScreen),
        Color.FromArgb("#061206"));

    public static readonly BindableProperty TargetsProperty = BindableProperty.Create(
        nameof(Targets),
        typeof(IReadOnlyList<RadarTarget>),
        typeof(RadarScreen),
        propertyChanged: HandleTargetsChanged);

    public float BorderMargin
    {
        get => (float)GetValue(BorderMarginProperty);
        set => SetValue(BorderMarginProperty, value);
    }

    public Color MemoryColor
    {
        get => (Color)GetValue(MemoryColorProperty);
        set => SetValue(MemoryColorProperty, value);
    }

    public float MemoryLineWidth
    {
        get => (float)GetValue(MemoryLineWidthProperty);
        set => SetValue(MemoryLineWidthProperty, value);
    }

    public float MemoryLengthShort
    {
        get => (float)GetValue(MemoryLengthShortProperty);
        set => SetValue(MemoryLengthShortProperty, value);
    }

    public float MemoryLengthLong
    {
        get => (float)GetValue(MemoryLengthLongProperty);
        set => SetValue(MemoryLengthLongProperty, value);
    }

    public float SweepAngle
    {
        get => (float)GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    public byte SweepArcAlpha
    {
        get => (byte)GetValue(SweepArcAlphaProperty);
        set => SetValue(SweepArcAlphaProperty, value);
    }

    public Color SweepColor
    {
        get => (Color)GetValue(SweepColorProperty);
        set => SetValue(SweepColorProperty, value);
    }

    public float SweepLineWidth
    {
        get => (float)GetValue(SweepLineWidthProperty);
        set => SetValue(SweepLineWidthProperty, value);
    }

    public Color ScreenColor
    {
        get => (Color)GetValue(ScreenColorProperty);
        set => SetValue(ScreenColorProperty, value);
    }

    public IReadOnlyList<RadarTarget>? Targets
    {
        get => (IReadOnlyList<RadarTarget>?)GetValue(TargetsProperty);
        set => SetValue(TargetsProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public RadarScreen()
    {
        Drawable = this;
        BackgroundColor = Colors.Black;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var cx = width / 2f;
        var cy = height / 2f;
        var center = new PointF(cx, cy);
        var radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2) - BorderMargin;

        canvas.Antialias = true;

        canvas.FillColor = ScreenColor;
        canvas.FillRectangle(0, 0, width, height);

        var sweepColor = SweepColor;
        var glowPaint = new RadialGradientPaint(
            [
                new PaintGradientStop(0f, new Color(sweepColor.Red, sweepColor.Green, sweepColor.Blue, GlowAlpha)),
                new PaintGradientStop(1f, new Color(sweepColor.Red, sweepColor.Green, sweepColor.Blue, 0f))
            ],
            center: new Point(0.5, 0.5),
            radius: 0.5);
        var glowRect = new RectF(cx - radius, cy - radius, radius * 2, radius * 2);
        canvas.SaveState();
        canvas.SetFillPaint(glowPaint, glowRect);
        canvas.FillEllipse(glowRect);
        canvas.RestoreState();

        canvas.StrokeColor = MemoryColor;
        canvas.StrokeSize = MemoryLineWidth;

        canvas.DrawEllipse(cx - radius, cy - radius, radius * 2, radius * 2);

        for (var i = 1; i <= RingCount; i++)
        {
            var r = radius * i / RingCount;
            canvas.DrawEllipse(cx - r, cy - r, r * 2, r * 2);
        }

        for (var a = 0; a < 360; a += CrossStep)
        {
            var (x, y) = GetPoint(center, radius, a);
            canvas.DrawLine(cx, cy, x, y);
        }

        for (var a = 0; a < 360; a += MemoryStep)
        {
            var outer = radius;
            var inner = radius - ((a % MemoryLongStep) == 0 ? MemoryLengthLong : MemoryLengthShort);
            var (x1, y1) = GetPoint(center, outer, a);
            var (x2, y2) = GetPoint(center, inner, a);
            canvas.DrawLine(x1, y1, x2, y2);
        }

        canvas.FontSize = CompassFontSize;
        canvas.FontColor = MemoryColor;
        var labelOffset = radius - MemoryLengthLong - CompassInset;
        canvas.DrawString("N", cx, cy - labelOffset + CompassBaseline, HorizontalAlignment.Center);
        canvas.DrawString("S", cx, cy + labelOffset + CompassBaseline, HorizontalAlignment.Center);
        canvas.DrawString("E", cx + labelOffset, cy + CompassBaseline, HorizontalAlignment.Center);
        canvas.DrawString("W", cx - labelOffset, cy + CompassBaseline, HorizontalAlignment.Center);

        var targets = Targets;
        if (targets is not null)
        {
            var now = Environment.TickCount64;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (!targetPassTimes.TryGetValue(target, out var passTime))
                {
                    continue;
                }

                var alpha = 1f - ((now - passTime) / TargetFadeDuration);
                if (alpha <= 0f)
                {
                    continue;
                }

                var r = radius * Math.Clamp(target.Distance, 0f, 1f);
                var (tx, ty) = GetPoint(center, r, target.Angle);

                canvas.FillColor = new Color(sweepColor.Red, sweepColor.Green, sweepColor.Blue, alpha * TargetHaloAlpha);
                canvas.FillCircle(tx, ty, TargetHaloRadius);
                canvas.FillColor = new Color(sweepColor.Red, sweepColor.Green, sweepColor.Blue, alpha);
                canvas.FillCircle(tx, ty, TargetRadius);
            }
        }

        canvas.FillColor = new Color(sweepColor.Red, sweepColor.Green, sweepColor.Blue, SweepArcAlpha / 256f);

        var endAngle = 360f - currentAngle;
        for (var i = 0; i < SweepAngle; i += SweepStep)
        {
            var startAngle = (endAngle + i) % 360f;

            var path = new PathF();
            path.MoveTo(cx, cy);
            path.AddArc(cx - radius, cy - radius, cx + radius, cy + radius, startAngle, endAngle, true);
            path.Close();

            canvas.FillPath(path);
        }

        canvas.StrokeColor = sweepColor;
        canvas.StrokeSize = SweepLineWidth;
        canvas.StrokeLineCap = LineCap.Round;

        var (ex, ey) = GetPoint(center, radius, currentAngle);

        canvas.DrawLine(cx, cy, ex, ey);

        canvas.StrokeColor = new Color(0f, 0f, 0f, ScanLineAlpha);
        canvas.StrokeSize = ScanLineWidth;
        for (var y = cy - radius; y <= cy + radius; y += ScanLineSpacing)
        {
            var dy = Math.Abs(y - cy);
            var half = (float)Math.Sqrt(Math.Max(0f, (radius * radius) - (dy * dy)));
            canvas.DrawLine(cx - half, y, cx + half, y);
        }

        var vignettePaint = new RadialGradientPaint(
            [
                new PaintGradientStop(VignetteStart, new Color(0f, 0f, 0f, 0f)),
                new PaintGradientStop(1f, new Color(0f, 0f, 0f, VignetteAlpha))
            ],
            center: new Point(0.5, 0.5),
            radius: VignetteRadius);
        canvas.SaveState();
        canvas.SetFillPaint(vignettePaint, dirtyRect);
        canvas.FillRectangle(dirtyRect);
        canvas.RestoreState();
    }

    private static PointF GetPoint(PointF center, float radius, float degrees)
    {
        var radians = Single.DegreesToRadians(degrees);
        return new PointF(center.X + (radius * MathF.Cos(radians)), center.Y + (radius * MathF.Sin(radians)));
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private void OnLoaded(object? sender, EventArgs e) => StartTimer();

    private void OnUnloaded(object? sender, EventArgs e) => StopTimer();

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler != null)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    private void StartTimer()
    {
        if ((cts is not null) && !cts.IsCancellationRequested)
        {
            return;
        }

        cts = new CancellationTokenSource();
        _ = Loop(cts.Token);
    }

    private void StopTimer()
    {
        if (cts is null)
        {
            return;
        }

        cts.Cancel();
        cts.Dispose();
        cts = null;
    }

    private async Task Loop(CancellationToken token)
    {
        try
        {
            using var timer = new PeriodicTimer(Interval);
            while (await timer.WaitForNextTickAsync(token))
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await Dispatcher.DispatchAsync(() =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    var prevAngle = currentAngle;
                    currentAngle += StepSpeed;
                    if (currentAngle >= 360f)
                    {
                        currentAngle -= 360f;
                    }

                    UpdateTargetPass(prevAngle, currentAngle);

                    Invalidate();
                });
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    //--------------------------------------------------------------------------------
    // Target
    //--------------------------------------------------------------------------------

    private static void HandleTargetsChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var screen = (RadarScreen)bindable;
        if (oldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= screen.HandleTargetsCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += screen.HandleTargetsCollectionChanged;
        }

        screen.targetPassTimes.Clear();
    }

    private void HandleTargetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            targetPassTimes.Clear();
        }
        else if (e.OldItems is not null)
        {
            foreach (var target in e.OldItems.OfType<RadarTarget>())
            {
                targetPassTimes.Remove(target);
            }
        }
    }

    private void UpdateTargetPass(float fromAngle, float toAngle)
    {
        var targets = Targets;
        if (targets is null)
        {
            return;
        }

        var now = Environment.TickCount64;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            var angle = NormalizeDegrees(target.Angle);
            if (IsAnglePassed(fromAngle, toAngle, angle))
            {
                targetPassTimes[target] = now;
            }
        }
    }

    private static float NormalizeDegrees(float degrees) => ((degrees % 360f) + 360f) % 360f;

    private static bool IsAnglePassed(float from, float to, float angle)
    {
        if (from <= to)
        {
            return (angle > from) && (angle <= to);
        }
        return (angle > from) || (angle <= to);
    }
}
#pragma warning restore CA1001
