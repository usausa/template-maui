namespace Template.MobileApp.Controls;

#pragma warning disable CA1001
public sealed class RadarControl : GraphicsView, IDrawable
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1000d / 60);

    private CancellationTokenSource? cts;

    public RadarControl()
    {
        Drawable = this;
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

    private async Task Loop(CancellationToken ct)
    {
        try
        {
            using var timer = new PeriodicTimer(Interval);
            while (await timer.WaitForNextTickAsync(ct))
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                // TODO
                MainThread.BeginInvokeOnMainThread(Invalidate);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
    }
}
#pragma warning restore CA1001
