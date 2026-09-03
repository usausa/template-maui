namespace Template.MobileApp.Controls;

#pragma warning disable CA1001
public sealed class RadarScreen : GraphicsView, IDrawable
{
    private const float StepSpeed = 2f;

    // 点灯から消灯までの時間(ミリ秒)。掃引周期(3 秒)より少し短くする
    private const float TargetFadeDuration = 2500f;

    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1000d / 60);

    private CancellationTokenSource? cts;

    private float currentAngle;

    private readonly Dictionary<RadarTarget, long> targetPassTimes = [];

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

    public static readonly BindableProperty ScreenColorProperty = BindableProperty.Create(
        nameof(ScreenColor),
        typeof(Color),
        typeof(RadarScreen),
        Color.FromArgb("#061206"));

    public Color ScreenColor
    {
        get => (Color)GetValue(ScreenColorProperty);
        set => SetValue(ScreenColorProperty, value);
    }

    public static readonly BindableProperty TargetsProperty = BindableProperty.Create(
        nameof(Targets),
        typeof(IReadOnlyList<RadarTarget>),
        typeof(RadarScreen),
        propertyChanged: HandleTargetsChanged);

    public IReadOnlyList<RadarTarget>? Targets
    {
        get => (IReadOnlyList<RadarTarget>?)GetValue(TargetsProperty);
        set => SetValue(TargetsProperty, value);
    }

    private static void HandleTargetsChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        ((RadarScreen)bindable).targetPassTimes.Clear();
    }

    public RadarScreen()
    {
        Drawable = this;
        BackgroundColor = Colors.Black;

        // Handlerがnullにならない経路でもループを止められるようLoaded/Unloadedを併用する
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

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

                MainThread.BeginInvokeOnMainThread(() =>
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

    // 掃引線が通過したターゲットの点灯時刻を記録する
    private void UpdateTargetPass(float fromAngle, float toAngle)
    {
        var targets = Targets;
        if (targets is null)
        {
            return;
        }

        var now = Environment.TickCount64;
        foreach (var target in targets)
        {
            var angle = ((target.Angle % 360f) + 360f) % 360f;
            if (IsAnglePassed(fromAngle, toAngle, angle))
            {
                targetPassTimes[target] = now;
            }
        }
    }

    private static bool IsAnglePassed(float from, float to, float angle)
    {
        if (from <= to)
        {
            return (angle > from) && (angle <= to);
        }
        // 360 度の折り返しをまたいだ場合
        return (angle > from) || (angle <= to);
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
        canvas.FillColor = ScreenColor;
        canvas.FillRectangle(0, 0, width, height);

        // Center glow (CRT の中心発光)
        var sweepColor = SweepColor;
        var glowPaint = new RadialGradientPaint(
            [
                new PaintGradientStop(0f, new Color(sweepColor.Red, sweepColor.Green, sweepColor.Blue, 0.20f)),
                new PaintGradientStop(1f, new Color(sweepColor.Red, sweepColor.Green, sweepColor.Blue, 0f))
            ],
            center: new Point(0.5, 0.5),
            radius: 0.5);
        var glowRect = new RectF(cx - radius, cy - radius, radius * 2, radius * 2);
        canvas.SaveState();
        canvas.SetFillPaint(glowPaint, glowRect);
        canvas.FillEllipse(glowRect);
        canvas.RestoreState();

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

        // Compass (N/E/S/W。画面座標系は 0 度=右・時計回り)
        canvas.FontSize = 16f;
        canvas.FontColor = MemoryColor;
        var labelOffset = radius - MemoryLengthLong - 18f;
        canvas.DrawString("N", cx, cy - labelOffset + 6f, HorizontalAlignment.Center);
        canvas.DrawString("S", cx, cy + labelOffset + 6f, HorizontalAlignment.Center);
        canvas.DrawString("E", cx + labelOffset, cy + 6f, HorizontalAlignment.Center);
        canvas.DrawString("W", cx - labelOffset, cy + 6f, HorizontalAlignment.Center);

        // Targets (掃引通過で点灯し時間経過でフェードアウト)
        var targets = Targets;
        if (targets is not null)
        {
            var now = Environment.TickCount64;
            foreach (var target in targets)
            {
                if (!targetPassTimes.TryGetValue(target, out var passTime))
                {
                    continue;
                }

                var alpha = 1f - ((now - passTime) / TargetFadeDuration);
                if (alpha <= 0f)
                {
                    continue;
                }

                var rad = DegreesToRadians(target.Angle);
                var r = radius * Math.Clamp(target.Distance, 0f, 1f);
                var tx = cx + (r * (float)Math.Cos(rad));
                var ty = cy + (r * (float)Math.Sin(rad));

                canvas.FillColor = new Color(sweepColor.Red, sweepColor.Green, sweepColor.Blue, alpha * 0.25f);
                canvas.FillCircle(tx, ty, 10f);
                canvas.FillColor = new Color(sweepColor.Red, sweepColor.Green, sweepColor.Blue, alpha);
                canvas.FillCircle(tx, ty, 5f);
            }
        }

        // Sweep arc
        canvas.FillColor = new Color(sweepColor.Red, sweepColor.Green, sweepColor.Blue, SweepArcAlpha / 256f);

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
        canvas.StrokeColor = sweepColor;
        canvas.StrokeSize = SweepLineWidth;
        canvas.StrokeLineCap = LineCap.Round;

        var radCurrent = DegreesToRadians(currentAngle);
        var ex = cx + (radius * (float)Math.Cos(radCurrent));
        var ey = cy + (radius * (float)Math.Sin(radCurrent));

        canvas.DrawLine(cx, cy, ex, ey);

        // Scan lines (CRT の走査線。円領域のみ)
        canvas.StrokeColor = new Color(0f, 0f, 0f, 0.18f);
        canvas.StrokeSize = 1f;
        for (var y = cy - radius; y <= cy + radius; y += 4f)
        {
            var dy = Math.Abs(y - cy);
            var half = (float)Math.Sqrt(Math.Max(0f, (radius * radius) - (dy * dy)));
            canvas.DrawLine(cx - half, y, cx + half, y);
        }

        // Vignette (四隅を暗く)
        var vignettePaint = new RadialGradientPaint(
            [
                new PaintGradientStop(0.7f, new Color(0f, 0f, 0f, 0f)),
                new PaintGradientStop(1f, new Color(0f, 0f, 0f, 0.55f))
            ],
            center: new Point(0.5, 0.5),
            radius: 0.72);
        canvas.SaveState();
        canvas.SetFillPaint(vignettePaint, dirtyRect);
        canvas.FillRectangle(dirtyRect);
        canvas.RestoreState();
    }
}
#pragma warning restore CA1001
