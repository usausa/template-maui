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

    private PeriodicTimer? timer;
    private CancellationTokenSource? cancellationTokenSource;
    private Task? loopTask;

    private readonly AtomicInteger stickX = new();
    private readonly AtomicInteger stickY = new();

    private readonly AtomicBoolean buttonA = new();
    private readonly AtomicBoolean buttonB = new();
    private readonly AtomicBoolean buttonX = new();
    private readonly AtomicBoolean buttonY = new();

    [ObservableProperty]
    public partial int Fps { get; set; }

    [ObservableProperty]
    public partial int Speed { get; set; }

    public int StickX
    {
        get => stickX.Value;
        set => stickX.Value = value;
    }

    public int StickY
    {
        get => stickY.Value;
        set => stickY.Value = value;
    }

    public bool ButtonA
    {
        get => buttonA.Value;
        set => buttonA.Value = value;
    }

    public bool ButtonB
    {
        get => buttonB.Value;
        set => buttonB.Value = value;
    }

    public bool ButtonX
    {
        get => buttonX.Value;
        set => buttonX.Value = value;
    }

    public bool ButtonY
    {
        get => buttonY.Value;
        set => buttonY.Value = value;
    }

    // スタック退避中もループが回り続けないよう、画面表示中のみ実行する
    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        // 二重呼び出しで前回のループを停止できなくなるのを防ぐ
        if (loopTask is not null)
        {
            return Task.CompletedTask;
        }

        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000d / 60));
        cancellationTokenSource = new CancellationTokenSource();
        loopTask = StartTimerAsync(timer, cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public override async Task OnNavigatingFromAsync(INavigationContext context)
    {
        if (loopTask is null)
        {
            return;
        }

        await cancellationTokenSource!.CancelAsync();
        try
        {
            await loopTask;
        }
        finally
        {
            // ループが想定外の例外で落ちても解放を確実に行う
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
            timer!.Dispose();
            timer = null;
            loopTask = null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 通常はOnNavigatingFromAsyncで解放済み。ここは保険
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            timer?.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task StartTimerAsync(PeriodicTimer periodicTimer, CancellationToken token)
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
            while (await periodicTimer.WaitForNextTickAsync(token))
            {
                // Speed
                var a = ButtonA;
                var b = ButtonB;

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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
