namespace Template.MobileApp.Shell;

using Smart.Mvvm.Resolver;

using System.Diagnostics;

public partial class DiagnosticPanel
{
    private const double MinFrameTime = 0.1;

    private const double EmaAlpha = 0.9;

    private readonly Stopwatch stopwatch = new();

    private readonly int processorCount = Environment.ProcessorCount;

    private readonly Process currentProcess = Process.GetCurrentProcess();

    private readonly IDisplay display;

    private bool isMonitoring;

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
        display.FrameUpdated += OnDisplayFrameUpdated;

        HandlerChanged += OnHandlerChanged;
    }

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((DiagnosticPanel)bindable).UpdateValues();
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        if (Handler is null)
        {
            StopMonitor();
        }
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(IsVisible))
        {
            StartMonitor();
        }
    }

    private void StartMonitor()
    {
        if (isMonitoring)
        {
            return;
        }

        cpuTimePrev = currentProcess.TotalProcessorTime;
        allocatedBytesPrev = GC.GetTotalAllocatedBytes();

        display.StartMonitor();
        stopwatch.Start();

        Application.Current!.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            UpdateValues();
            return isMonitoring;
        });

        isMonitoring = true;
    }

    private void StopMonitor()
    {
        if (!isMonitoring)
        {
            return;
        }

        display.StartMonitor();
        stopwatch.Stop();

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
        // CPU
        var cpuTimeCurrent = currentProcess.TotalProcessorTime;
        var cpuUsage = ((cpuTimeCurrent - cpuTimePrev).TotalMilliseconds / stopwatch.Elapsed.TotalMilliseconds) * 100 / processorCount;
        cpuTimePrev = cpuTimeCurrent;

        // Thread
        var threads = currentProcess.Threads.Count;

        // Memory
        var memoryUsed = (float)currentProcess.WorkingSet64 / (1024 * 1024);

        // Allocation
        var elapsedSec = stopwatch.Elapsed.TotalSeconds;
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
        MemoryLabel.Text = $"{memoryUsed:F1} MB";
        MemoryLabel.TextColor = memoryUsed switch
        {
            <= 256.0f => safeColor,
            <= 512.0f => warningColor,
            _ => criticalColor
        };

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
    }
}
