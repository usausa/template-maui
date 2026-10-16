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

    ValueTask StopAsync();
}

public sealed partial class NoiseMonitor : INoiseMonitor
{
    public event EventHandler<NoiseEventArgs>? Measured;

    private CancellationTokenSource? cts;

    private Task? loopTask;

    public bool IsRunning => loopTask is { IsCompleted: false };

    public void Dispose()
    {
        StopAsync().AsTask().GetAwaiter().GetResult();
        cts?.Dispose();
    }

    public void Start(int interval = 200)
    {
        if (IsRunning)
        {
            return;
        }

        cts?.Dispose();

        cts = new CancellationTokenSource();
        loopTask = Loop(interval, cts.Token);
    }

    public async ValueTask StopAsync()
    {
        if (loopTask is null)
        {
            return;
        }

        await cts!.CancelAsync().ConfigureAwait(false);
        try
        {
            await loopTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }

        cts.Dispose();
        cts = null;
        loopTask = null;
    }

    private async Task Loop(int interval, CancellationToken token)
    {
        try
        {
            if (!SetupMeasure())
            {
                return;
            }

            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(interval));
            do
            {
                var value = await Measure().ConfigureAwait(false);
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

    private partial bool SetupMeasure();

    private partial void CleanupMeasure();

    private partial ValueTask<double> Measure();
}
