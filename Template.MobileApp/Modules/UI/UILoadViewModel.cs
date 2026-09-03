namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Components;
using Template.MobileApp.Graphics.Drawing;

public sealed partial class UILoadViewModel : AppViewModelBase
{
    private static readonly TimeSpan PeakWindow = TimeSpan.FromSeconds(3);

    private readonly INoiseMonitor noiseMonitor;

    private readonly Queue<(DateTime Timestamp, double Value)> peakHistory = new();

    [ObservableProperty]
    public partial double Current { get; set; }

    [ObservableProperty]
    public partial double Average { get; set; }

    [ObservableProperty]
    public partial double Min { get; set; }

    [ObservableProperty]
    public partial double Max { get; set; }

    [ObservableProperty]
    public partial double Peak { get; set; }

    public LoadDrawing Drawing { get; } = new();

    public UILoadViewModel(INoiseMonitor noiseMonitor)
    {
        this.noiseMonitor = noiseMonitor;

        Disposables.Add(noiseMonitor.MeasuredAsObservable().ObserveOnCurrentContext().Subscribe(x =>
        {
            Current = x.Decibel;
            Drawing.AddValue((float)x.Decibel);
            var (avg, min, max) = Drawing.CalcStatics();
            Average = avg;
            Min = min;
            Max = max;
            Peak = CalcPeak(x.Decibel);
        }));
    }

    // 直近 3 秒間の最大値を保持する
    private double CalcPeak(double value)
    {
        var now = DateTime.Now;
        peakHistory.Enqueue((now, value));

        var limit = now - PeakWindow;
        while ((peakHistory.Count > 0) && (peakHistory.Peek().Timestamp < limit))
        {
            peakHistory.Dequeue();
        }

        var peak = 0d;
        foreach (var (_, entryValue) in peakHistory)
        {
            if (entryValue > peak)
            {
                peak = entryValue;
            }
        }
        return peak;
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (await Permissions.RequestMicrophoneAsync())
        {
            noiseMonitor.Start();
        }
    }

    public override async Task OnNavigatingFromAsync(INavigationContext context)
    {
        await noiseMonitor.StopAsync();
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4()
    {
        Drawing.Clear();
        peakHistory.Clear();
        Current = 0;
        Average = 0;
        Min = 0;
        Max = 0;
        Peak = 0;
        return Task.CompletedTask;
    }
}
