namespace Template.MobileApp.Modules.UI;

using System.Diagnostics;

using Smart.Threading;

public sealed partial class UIMeterViewModel : AppViewModelBase
{
    private const double AccelVelocity1 = 64d / 60;
    private const double AccelVelocity2 = 48d / 60;
    private const double AccelVelocity3 = 32d / 60;
    private const double AccelVelocity4 = 16d / 60;
    private const double BrakeVelocity = 96d / 60;
    private const double DefaultVelocity = 32d / 60;

    private readonly PeriodicTimer timer;
    private readonly CancellationTokenSource cancellationTokenSource;

    private readonly AtomicBoolean accelerator = new();
    private readonly AtomicBoolean brake = new();

    [ObservableProperty]
    public partial int Fps { get; set; }

    [ObservableProperty]
    public partial int Speed { get; set; }

    public bool Accelerator
    {
        get => accelerator.Value;
        set => accelerator.Value = value;
    }

    public bool Brake
    {
        get => brake.Value;
        set => brake.Value = value;
    }

    public UIMeterViewModel()
    {
        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000d / 60));
        cancellationTokenSource = new CancellationTokenSource();

        _ = StartTimerAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            timer.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task StartTimerAsync()
    {
        try
        {
            // Low resolution
            var fps = 0;
            var speed = 0d;
            var prevSpeed = 0;
            var prevAccel = false;
            var prevBrake = false;

            var watch = Stopwatch.StartNew();
            while (await timer.WaitForNextTickAsync(cancellationTokenSource.Token))
            {
                // Speed
                var a = Accelerator;
                var b = Brake;

                if (b)
                {
                    speed = Math.Max(0, speed - BrakeVelocity);
                }
                else if (a)
                {
                    var velocity = speed switch
                    {
                        < 128 => AccelVelocity1,
                        < 192 => AccelVelocity2,
                        < 224 => AccelVelocity3,
                        _ => AccelVelocity4
                    };
                    speed = Math.Min(255, speed + velocity);
                }
                else
                {
                    speed = Math.Max(0, speed - DefaultVelocity);
                }

                var currentSpeed = (int)speed;
                if ((currentSpeed != prevSpeed) || (a != prevAccel) || (b != prevBrake))
                {
                    MainThread.BeginInvokeOnMainThread(() => Speed = currentSpeed);

                    prevSpeed = currentSpeed;
                    prevAccel = a;
                    prevBrake = b;
                }

                // FPS
                fps++;
                if (watch.ElapsedMilliseconds > 1000)
                {
                    var f = fps;
                    MainThread.BeginInvokeOnMainThread(() => Fps = f);

                    fps = 0;
                    watch.Restart();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
