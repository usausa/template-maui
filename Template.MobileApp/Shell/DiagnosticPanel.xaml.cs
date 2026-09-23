namespace Template.MobileApp.Shell;

using System.Diagnostics;

using Smart.Mvvm.Resolver;

public partial class DiagnosticPanel
{
    private const double MinFrameTime = 0.1;

    private const double EmaAlpha = 0.9;

    private const int MemoryHistoryLength = 60;

    private const string DiagnosticClassId = "Diagnostic";

    private const int LayoutSuppressMilliseconds = 100;

    private readonly Stopwatch stopwatch = new();

    private readonly int processorCount = Environment.ProcessorCount;

    private readonly IDisplay display;

    private readonly MemorySparkline memorySparkline = new();

    private Process? currentProcess;

    private LayoutMetrics? layoutMetrics;

    private bool isMonitoring;

    private int monitorGeneration;

    private double emaFps;

    private TimeSpan cpuTimePrev;

    private long allocatedBytesPrev;

    private int gc0Prev;
    private int gc1Prev;
    private int gc2Prev;

    public static readonly BindableProperty SafeColorProperty = BindableProperty.Create(
        nameof(SafeColor),
        typeof(Color),
        typeof(DiagnosticPanel),
        Colors.Green,
        propertyChanged: OnPropertyChanged);

    public Color SafeColor
    {
        get => (Color)GetValue(SafeColorProperty);
        set => SetValue(SafeColorProperty, value);
    }

    public static readonly BindableProperty WarningColorProperty = BindableProperty.Create(
        nameof(WarningColor),
        typeof(Color),
        typeof(DiagnosticPanel),
        Colors.Orange,
        propertyChanged: OnPropertyChanged);

    public Color WarningColor
    {
        get => (Color)GetValue(WarningColorProperty);
        set => SetValue(WarningColorProperty, value);
    }

    public static readonly BindableProperty CriticalColorProperty = BindableProperty.Create(
        nameof(CriticalColor),
        typeof(Color),
        typeof(DiagnosticPanel),
        Colors.Red,
        propertyChanged: OnPropertyChanged);

    public Color CriticalColor
    {
        get => (Color)GetValue(CriticalColorProperty);
        set => SetValue(CriticalColorProperty, value);
    }

    public DiagnosticPanel()
    {
        InitializeComponent();

        display = ResolveProvider.Default.GetRequiredService<IDisplay>();
        MemoryChart.Drawable = memorySparkline;

        ClassId = DiagnosticClassId;
        foreach (var element in this.GetVisualTreeDescendants().OfType<Element>())
        {
            element.ClassId = DiagnosticClassId;
        }

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((DiagnosticPanel)bindable).UpdateValues();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        currentProcess ??= Process.GetCurrentProcess();
        display.FrameUpdated += OnDisplayFrameUpdated;

        if (IsVisible)
        {
            StartMonitor();
        }
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        StopMonitor();

        display.FrameUpdated -= OnDisplayFrameUpdated;
        currentProcess?.Dispose();
        currentProcess = null;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(IsVisible))
        {
            if (IsVisible && IsLoaded)
            {
                StartMonitor();
            }
            else
            {
                StopMonitor();
            }
        }
    }

    private void StartMonitor()
    {
        if (isMonitoring || (currentProcess is null))
        {
            return;
        }

        cpuTimePrev = currentProcess.TotalProcessorTime;
        allocatedBytesPrev = GC.GetTotalAllocatedBytes();
        memorySparkline.Clear();
        layoutMetrics = LayoutMetrics.Start(DiagnosticClassId);

        display.StartMonitor();
        stopwatch.Restart();

        var generation = ++monitorGeneration;
        Application.Current!.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (!isMonitoring || (generation != monitorGeneration))
            {
                return false;
            }

            UpdateValues();
            return true;
        });

        isMonitoring = true;
    }

    private void StopMonitor()
    {
        if (!isMonitoring)
        {
            return;
        }

        display.StopMonitor();
        stopwatch.Stop();
        layoutMetrics?.Dispose();
        layoutMetrics = null;

        isMonitoring = false;
    }

    private void OnDisplayFrameUpdated(double frameTimeMs)
    {
        frameTimeMs = Math.Max(frameTimeMs, MinFrameTime);
        var fps = 1000.0 / frameTimeMs;

        emaFps = emaFps == 0 ? fps : (EmaAlpha * emaFps) + ((1 - EmaAlpha) * fps);
    }

    private void UpdateValues()
    {
        if (currentProcess is null)
        {
            return;
        }

        // 前回計測からの経過時間で割るため計測ごとにリセットする
        var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        stopwatch.Restart();

        // CPU
        var cpuTimeCurrent = currentProcess.TotalProcessorTime;
        var cpuUsage = elapsedMs > 0 ? (cpuTimeCurrent - cpuTimePrev).TotalMilliseconds / elapsedMs * 100 / processorCount : 0;
        cpuTimePrev = cpuTimeCurrent;

        // Thread
        var threads = currentProcess.Threads.Count;

        // Memory
        var memoryUsed = (float)currentProcess.WorkingSet64 / (1024 * 1024);

        // Allocation
        var elapsedSec = elapsedMs / 1000;
        if (elapsedSec <= 0)
        {
            elapsedSec = 1; // fallback
        }

        var currentAllocated = GC.GetTotalAllocatedBytes();
        var allocatedPerSec = ((currentAllocated - allocatedBytesPrev) / (1024.0 * 1024.0)) / elapsedSec; // MB/sec
        allocatedBytesPrev = currentAllocated;

        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);
        var gc0Delta = gen0 - gc0Prev;
        var gc1Delta = gen1 - gc1Prev;
        var gc2Delta = gen2 - gc2Prev;
        gc0Prev = gen0;
        gc1Prev = gen1;
        gc2Prev = gen2;

        // Update
        var safeColor = SafeColor;
        var warningColor = WarningColor;
        var criticalColor = CriticalColor;

        // FPS
        FpsLabel.Text = $"{emaFps:F1}";
        FpsLabel.TextColor = emaFps switch
        {
            >= 50 => safeColor,
            >= 30 => warningColor,
            _ => criticalColor
        };

        // CPU
        CpuLabel.Text = $"{cpuUsage:F1} %";
        CpuLabel.TextColor = cpuUsage switch
        {
            <= 30.0f => safeColor,
            <= 60.0f => warningColor,
            _ => criticalColor
        };

        // Thread
        ThreadsLabel.Text = $"{threads}";
        ThreadsLabel.TextColor = threads switch
        {
            <= 64 => safeColor,
            <= 128 => warningColor,
            _ => criticalColor
        };

        // Memory
        var memoryColor = memoryUsed switch
        {
            <= 256.0f => safeColor,
            <= 512.0f => warningColor,
            _ => criticalColor
        };
        MemoryLabel.Text = $"{memoryUsed:F1} MB";
        MemoryLabel.TextColor = memoryColor;
        memorySparkline.Add(memoryUsed, memoryColor);
        MemoryChart.Invalidate();

        Gc0Label.Text = $"{gc0Delta}";
        Gc1Label.Text = $"{gc1Delta}";
        Gc2Label.Text = $"{gc2Delta}";
        var gcColor = (gc0Delta + gc1Delta + gc2Delta) switch
        {
            0 => safeColor,
            _ => criticalColor
        };
        Gc0Label.TextColor = gcColor;
        Gc1Label.TextColor = gcColor;
        Gc2Label.TextColor = gcColor;

        AllocLabel.Text = $"{allocatedPerSec:F1} MB";
        AllocLabel.TextColor = allocatedPerSec switch
        {
            <= 4.0f => safeColor,
            <= 8.0f => warningColor,
            _ => criticalColor
        };

        // Layout
        if (layoutMetrics is not null)
        {
            var (measures, arranges) = layoutMetrics.Take();
            MeasureLabel.Text = $"{measures}";
            MeasureLabel.TextColor = LayoutColor(measures);
            ArrangeLabel.Text = $"{arranges}";
            ArrangeLabel.TextColor = LayoutColor(arranges);
            layoutMetrics.Suppress(LayoutSuppressMilliseconds);
        }

        Color LayoutColor(long count) => count switch
        {
            0 => safeColor,
            <= 500 => warningColor,
            _ => criticalColor
        };
    }

    private sealed class MemorySparkline : IDrawable
    {
        private static readonly Color GridColor = Color.FromArgb("#E0E0E0");

        private readonly List<double> values = [];

        private Color lineColor = Colors.Green;

        public void Clear() => values.Clear();

        public void Add(double value, Color color)
        {
            if (values.Count >= MemoryHistoryLength)
            {
                values.RemoveAt(0);
            }

            values.Add(value);
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

            if (values.Count < 2)
            {
                return;
            }

            var min = values.Min();
            var max = values.Max();
            var range = Math.Max(1d, max - min);
            var low = ((min + max) / 2) - (range / 2);
            var step = dirtyRect.Width / (MemoryHistoryLength - 1);
            var top = dirtyRect.Top + 2f;
            var height = dirtyRect.Height - 4f;

            PointF ToPoint(int i) =>
                new(
                    dirtyRect.Right - (step * (values.Count - 1 - i)),
                    (float)(top + height - ((values[i] - low) / range * height)));

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
        }
    }
}
