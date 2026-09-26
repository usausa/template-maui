namespace Template.MobileApp.Modules.Network;

using System.Diagnostics;

using Template.MobileApp.Diagnostics;
using Template.MobileApp.Usecase;

public sealed class NetworkTelemetryViewModel : AppViewModelBase
{
    private readonly ILogger<NetworkTelemetryViewModel> log;

    private readonly IDialog dialog;

    private readonly IDispatcher dispatcher;

    private readonly DiagnosticsInstrumentation instrumentation;

    public IObserveCommand WarningCommand { get; }
    public IObserveCommand ErrorCommand { get; }
    public IObserveCommand SpanCommand { get; }
    public IObserveCommand NetworkCommand { get; }
    public IObserveCommand FlushCommand { get; }
    public IObserveCommand CrashCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public NetworkTelemetryViewModel(
        ILogger<NetworkTelemetryViewModel> log,
        IDialog dialog,
        IDispatcher dispatcher,
        ITelemetryControl telemetryControl,
        DiagnosticsInstrumentation instrumentation,
        NetworkUsecase networkUsecase)
    {
        this.log = log;
        this.dialog = dialog;
        this.dispatcher = dispatcher;
        this.instrumentation = instrumentation;

        WarningCommand = MakeDelegateCommand(log.WarnTelemetryTest);
        ErrorCommand = MakeDelegateCommand(LogError);
        SpanCommand = MakeAsyncCommand(RunSpanAsync);
        NetworkCommand = MakeAsyncCommand(() => networkUsecase.GetServerTimeAsync().AsTask());
        FlushCommand = MakeDelegateCommand(telemetryControl.Flush);
        CrashCommand = MakeAsyncCommand(CrashAsync);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    // 例外の属性 (exception.stacktrace) を確かめるため、投げて捕まえた例外を記録する
    private void LogError()
    {
        try
        {
            throw new InvalidOperationException("Telemetry test.");
        }
        catch (InvalidOperationException ex)
        {
            log.ErrorTelemetryTest(ex);
        }
    }

    // 親子のスパンと、スパンの中のログ (同じトレース ID が付く)
    private async Task RunSpanAsync()
    {
        // The parent is Activity.Current when the context is default
        using var activity = instrumentation.Source.StartActivity("TelemetryTest", ActivityKind.Internal, default(ActivityContext));
        var watch = Stopwatch.StartNew();
        using (instrumentation.Source.StartActivity("Compute", ActivityKind.Internal, default(ActivityContext)))
        {
            await Task.Delay(100);
        }

        log.WarnTelemetryTestSpan(watch.ElapsedMilliseconds);
    }

    private async Task CrashAsync()
    {
        if (await dialog.ConfirmAsync("Crash the application ?"))
        {
            ThrowOnMainThread();
        }
    }

    // Throw on the UI thread outside of the command so that it is not handled
    private void ThrowOnMainThread() =>
        dispatcher.Dispatch(static () => throw new InvalidOperationException("Crash test."));
}
