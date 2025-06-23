namespace Template.MobileApp.Components;

public sealed class NoiseEventArgs : EventArgs
{
    public double Decibel { get; }

    public NoiseEventArgs(double decibel)
    {
        Decibel = decibel;
    }
}

public interface INoiseMonitor : IDisposable
{
    event EventHandler<NoiseEventArgs>? Measured;

    bool IsRunning { get; }

    void Start(int interval = 200);

    void Stop();
}

public sealed partial class NoiseMonitor : INoiseMonitor
{
    public event EventHandler<NoiseEventArgs>? Measured;

    private CancellationTokenSource? cts;

    public bool IsRunning => (cts is not null) && !cts.IsCancellationRequested;

    public void Dispose()
    {
        cts?.Dispose();
    }

    // ReSharper disable once AsyncVoidMethod
    public void Start(int interval = 200)
    {
        if (IsRunning)
        {
            return;
        }

        cts = new CancellationTokenSource();
        _ = Loop(interval, cts.Token);
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        cts!.Cancel();
        cts.Dispose();
        cts = null;
    }

    private async Task Loop(int interval, CancellationToken token)
    {
        SetupMeasure();
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(interval));
            do
            {
                var value = await Measure();
                if (value > 0)
                {
                    Measured?.Invoke(this, new NoiseEventArgs(value));
                }
            }
            while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        finally
        {
            CleanupMeasure();
        }
    }

    private partial void SetupMeasure();

    private partial void CleanupMeasure();

    private partial ValueTask<double> Measure();
}
