namespace Template.MobileApp.Controls;

#pragma warning disable CA1001
public sealed class RadarScreen : GraphicsView, IDrawable
{
    private const float StepSpeed = 2f;

    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1000d / 60);

    private CancellationTokenSource? cts;

    private float currentAngle;

    public static readonly BindableProperty BorderMarginProperty = BindableProperty.Create(
        nameof(BorderMargin),
        typeof(float),
        typeof(RadarScreen),
        10f);

    public float BorderMargin
    {
        get => (float)GetValue(BorderMarginProperty);
        set => SetValue(BorderMarginProperty, value);
    }

    public static readonly BindableProperty MemoryColorProperty = BindableProperty.Create(
        nameof(MemoryColor),
        typeof(Color),
        typeof(RadarScreen),
        Colors.Green);

    public Color MemoryColor
    {
        get => (Color)GetValue(MemoryColorProperty);
        set => SetValue(MemoryColorProperty, value);
    }

    public static readonly BindableProperty MemoryLineWidthProperty = BindableProperty.Create(
        nameof(MemoryLineWidth),
        typeof(float),
        typeof(RadarScreen),
        3f);

    public float MemoryLineWidth
    {
        get => (float)GetValue(MemoryLineWidthProperty);
        set => SetValue(MemoryLineWidthProperty, value);
    }

    public static readonly BindableProperty MemoryLengthShortProperty = BindableProperty.Create(
        nameof(MemoryLengthShort),
        typeof(float),
        typeof(RadarScreen),
        8f);

    public float MemoryLengthShort
    {
        get => (float)GetValue(MemoryLengthShortProperty);
        set => SetValue(MemoryLengthShortProperty, value);
    }

    public static readonly BindableProperty MemoryLengthLongProperty = BindableProperty.Create(
        nameof(MemoryLengthLong),
        typeof(float),
        typeof(RadarScreen),
        16f);

    public float MemoryLengthLong
    {
        get => (float)GetValue(MemoryLengthLongProperty);
        set => SetValue(MemoryLengthLongProperty, value);
    }

    public static readonly BindableProperty SweepAngleProperty = BindableProperty.Create(
        nameof(SweepAngle),
        typeof(float),
        typeof(RadarScreen),
        60f);

    public float SweepAngle
    {
        get => (float)GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    public static readonly BindableProperty SweepArcAlphaProperty = BindableProperty.Create(
        nameof(SweepArcAlpha),
        typeof(byte),
        typeof(RadarScreen),
        (byte)12);

    public byte SweepArcAlpha
    {
        get => (byte)GetValue(SweepArcAlphaProperty);
        set => SetValue(SweepArcAlphaProperty, value);
    }

    public static readonly BindableProperty SweepColorProperty = BindableProperty.Create(
        nameof(SweepColor),
        typeof(Color),
        typeof(RadarScreen),
        Colors.Lime);

    public Color SweepColor
    {
        get => (Color)GetValue(SweepColorProperty);
        set => SetValue(SweepColorProperty, value);
    }

    public static readonly BindableProperty SweepLineWidthProperty = BindableProperty.Create(
        nameof(SweepLineWidth),
        typeof(float),
        typeof(RadarScreen),
        3f);

    public float SweepLineWidth
    {
        get => (float)GetValue(SweepLineWidthProperty);
        set => SetValue(SweepLineWidthProperty, value);
    }

    public RadarScreen()
    {
        Drawable = this;
        BackgroundColor = Colors.Black;
    }

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

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    currentAngle += StepSpeed;
                    if (currentAngle >= 360f)
                    {
                        currentAngle -= 360f;
                    }

                    Invalidate();
                });
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    private static float DegreesToRadians(float degrees) =>
        degrees * ((float)Math.PI / 180f);

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var cx = width / 2f;
        var cy = height / 2f;
        var radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2) - BorderMargin;

        canvas.Antialias = true;

        // Background
        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle(0, 0, width, height);

        // Circle
        canvas.StrokeColor = MemoryColor;
        canvas.StrokeSize = MemoryLineWidth;

        // Outer
        canvas.DrawEllipse(cx - radius, cy - radius, radius * 2, radius * 2);

        // Inner
        for (var i = 1; i <= 3; i++)
        {
            var r = radius * i / 3f;
            canvas.DrawEllipse(cx - r, cy - r, r * 2, r * 2);
        }

        // Cross line
        for (var a = 0; a < 360; a += 45)
        {
            var rad = DegreesToRadians(a);
            var x = cx + (radius * (float)Math.Cos(rad));
            var y = cy + (radius * (float)Math.Sin(rad));
            canvas.DrawLine(cx, cy, x, y);
        }

        // Memory
        for (var a = 0; a < 360; a += 5)
        {
            var rad = DegreesToRadians(a);
            var outer = radius;
            var inner = radius - ((a % 10) == 0 ? MemoryLengthLong : MemoryLengthShort);
            var x1 = cx + (outer * (float)Math.Cos(rad));
            var y1 = cy + (outer * (float)Math.Sin(rad));
            var x2 = cx + (inner * (float)Math.Cos(rad));
            var y2 = cy + (inner * (float)Math.Sin(rad));
            canvas.DrawLine(x1, y1, x2, y2);
        }

        // Sweep arc
        var c = SweepColor;
        canvas.FillColor = new Color(c.Red, c.Green, c.Blue, SweepArcAlpha / 256f);

        var endAngle = 360f - currentAngle;
        for (var i = 0; i < SweepAngle; i += 2)
        {
            var startAngle = (endAngle + i) % 360f;

            var path = new PathF();
            path.MoveTo(cx, cy);
            path.AddArc(cx - radius, cy - radius, cx + radius, cy + radius, startAngle, endAngle, true);
            path.Close();

            canvas.FillPath(path);
        }

        // Sweep line
        canvas.StrokeColor = SweepColor;
        canvas.StrokeSize = SweepLineWidth;
        canvas.StrokeLineCap = LineCap.Round;

        var radCurrent = DegreesToRadians(currentAngle);
        var ex = cx + (radius * (float)Math.Cos(radCurrent));
        var ey = cy + (radius * (float)Math.Sin(radCurrent));

        canvas.DrawLine(cx, cy, ex, ey);
    }
}
#pragma warning restore CA1001
