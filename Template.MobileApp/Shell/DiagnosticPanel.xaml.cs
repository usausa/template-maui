namespace Template.MobileApp.Shell;

using Template.MobileApp.Helpers;

public partial class DiagnosticPanel
{
    private const double MegaByte = 1024 * 1024;

    private readonly MemorySparkline memorySparkline = new();

    public static readonly BindableProperty SamplerProperty = BindableProperty.Create(
        nameof(Sampler),
        typeof(DiagnosticSampler),
        typeof(DiagnosticPanel),
        propertyChanged: static (bindable, oldValue, newValue) => ((DiagnosticPanel)bindable).OnSamplerChanged((DiagnosticSampler?)oldValue, (DiagnosticSampler?)newValue));

    public DiagnosticSampler? Sampler
    {
        get => (DiagnosticSampler?)GetValue(SamplerProperty);
        set => SetValue(SamplerProperty, value);
    }

    public static readonly BindableProperty SafeColorProperty = BindableProperty.Create(
        nameof(SafeColor),
        typeof(Color),
        typeof(DiagnosticPanel),
        Colors.Green);

    public Color SafeColor
    {
        get => (Color)GetValue(SafeColorProperty);
        set => SetValue(SafeColorProperty, value);
    }

    public static readonly BindableProperty WarningColorProperty = BindableProperty.Create(
        nameof(WarningColor),
        typeof(Color),
        typeof(DiagnosticPanel),
        Colors.Orange);

    public Color WarningColor
    {
        get => (Color)GetValue(WarningColorProperty);
        set => SetValue(WarningColorProperty, value);
    }

    public static readonly BindableProperty CriticalColorProperty = BindableProperty.Create(
        nameof(CriticalColor),
        typeof(Color),
        typeof(DiagnosticPanel),
        Colors.Red);

    public Color CriticalColor
    {
        get => (Color)GetValue(CriticalColorProperty);
        set => SetValue(CriticalColorProperty, value);
    }

    public DiagnosticPanel()
    {
        InitializeComponent();

        MemoryChart.Drawable = memorySparkline;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e) => Attach(Sampler);

    private void OnUnloaded(object? sender, EventArgs e) => Detach(Sampler);

    private void OnSamplerChanged(DiagnosticSampler? oldValue, DiagnosticSampler? newValue)
    {
        newValue?.ExcludeLayout(this);

        if (IsLoaded)
        {
            Detach(oldValue);
            Attach(newValue);
        }
    }

    private void Attach(DiagnosticSampler? sampler)
    {
        if (sampler is not null)
        {
            sampler.Sampled += OnSampled;
        }
    }

    private void Detach(DiagnosticSampler? sampler)
    {
        if (sampler is not null)
        {
            sampler.Sampled -= OnSampled;
        }
    }

    private void OnSampled(object? sender, EventArgs e)
    {
        if (IsVisible && (Sampler is { } sampler))
        {
            Render(sampler.Snapshot);
        }
    }

    private void Render(DiagnosticSnapshot snapshot)
    {
        // FPS
        FpsLabel.Text = $"{snapshot.Fps:F1}";
        FpsLabel.TextColor = ColorOf(snapshot.FpsLevel);

        // CPU
        CpuLabel.Text = $"{snapshot.CpuUsage:F1} %";
        CpuLabel.TextColor = ColorOf(snapshot.CpuLevel);

        // Thread
        ThreadsLabel.Text = $"{snapshot.ThreadCount}";
        ThreadsLabel.TextColor = ColorOf(snapshot.ThreadLevel);

        // Memory
        var memoryColor = ColorOf(snapshot.MemoryLevel);
        MemoryLabel.Text = $"{snapshot.WorkingSet / MegaByte:F1} MB";
        MemoryLabel.TextColor = memoryColor;
        memorySparkline.Update(snapshot.MemoryHistory, memoryColor);
        MemoryChart.Invalidate();

        // GC
        var gcColor = ColorOf(snapshot.GcLevel);
        Gc0Label.Text = $"{snapshot.Gc0Delta}";
        Gc1Label.Text = $"{snapshot.Gc1Delta}";
        Gc2Label.Text = $"{snapshot.Gc2Delta}";
        Gc0Label.TextColor = gcColor;
        Gc1Label.TextColor = gcColor;
        Gc2Label.TextColor = gcColor;

        // Allocation
        AllocLabel.Text = $"{snapshot.AllocationRate / MegaByte:F1} MB";
        AllocLabel.TextColor = ColorOf(snapshot.AllocationLevel);

        // Layout
        MeasureLabel.Text = $"{snapshot.MeasureCount}";
        MeasureLabel.TextColor = ColorOf(snapshot.MeasureLevel);
        ArrangeLabel.Text = $"{snapshot.ArrangeCount}";
        ArrangeLabel.TextColor = ColorOf(snapshot.ArrangeLevel);

        // Battery
        BatteryLabel.Text = snapshot.BatteryCharge is { } charge ? $"{charge * 100:F0} %" : "-";
        BatteryLabel.TextColor = ColorOf(snapshot.BatteryLevel);

        // WiFi
        WiFiLabel.Text = snapshot.WiFiSignalStrength is { } signalStrength ? $"{signalStrength} dBm" : "-";
        WiFiLabel.TextColor = ColorOf(snapshot.WiFiLevel);

        return;

        Color ColorOf(DiagnosticLevel level) => level switch
        {
            DiagnosticLevel.Safe => SafeColor,
            DiagnosticLevel.Warning => WarningColor,
            _ => CriticalColor
        };
    }

    private sealed class MemorySparkline : IDrawable
    {
        private static readonly Color GridColor = Color.FromArgb("#E0E0E0");

        private RingBuffer<double>? values;

        private Color lineColor = Colors.Green;

        public void Update(RingBuffer<double> history, Color color)
        {
            values = history;
            lineColor = color;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.Antialias = true;
            canvas.StrokeSize = 1f;
            canvas.StrokeColor = GridColor;
            canvas.DrawLine(dirtyRect.Left, dirtyRect.Top + 0.5f, dirtyRect.Right, dirtyRect.Top + 0.5f);
            canvas.DrawLine(dirtyRect.Left, dirtyRect.Center.Y, dirtyRect.Right, dirtyRect.Center.Y);
            canvas.DrawLine(dirtyRect.Left, dirtyRect.Bottom - 0.5f, dirtyRect.Right, dirtyRect.Bottom - 0.5f);

            if ((values is null) || (values.Count < 2))
            {
                return;
            }

            var min = values[0];
            var max = values[0];
            for (var i = 1; i < values.Count; i++)
            {
                min = Math.Min(min, values[i]);
                max = Math.Max(max, values[i]);
            }

            var range = Math.Max(1d, max - min);
            var low = ((min + max) / 2) - (range / 2);
            var step = dirtyRect.Width / (DiagnosticSampler.MemoryHistoryLength - 1);
            var top = dirtyRect.Top + 2f;
            var height = dirtyRect.Height - 4f;

            using var fill = new PathF();
            fill.MoveTo(ToPoint(0).X, dirtyRect.Bottom);
            for (var i = 0; i < values.Count; i++)
            {
                fill.LineTo(ToPoint(i));
            }

            fill.LineTo(dirtyRect.Right, dirtyRect.Bottom);
            fill.Close();
            canvas.FillColor = lineColor.WithAlpha(0.15f);
            canvas.FillPath(fill);

            using var line = new PathF();
            line.MoveTo(ToPoint(0));
            for (var i = 1; i < values.Count; i++)
            {
                line.LineTo(ToPoint(i));
            }

            canvas.StrokeSize = 2f;
            canvas.StrokeColor = lineColor;
            canvas.StrokeLineJoin = LineJoin.Round;
            canvas.DrawPath(line);

            return;

            PointF ToPoint(int i) => new(dirtyRect.Right - (step * (values.Count - 1 - i)), (float)(top + height - ((values[i] - low) / range * height)));
        }
    }
}
