namespace Template.MobileApp;

using CommunityToolkit.Maui.Core;

using Template.MobileApp.Components;
using Template.MobileApp.Diagnostics;
using Template.MobileApp.Modules;
using Template.MobileApp.Shell;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class MainPageViewModel : ExtendViewModelBase, IShellControl, IAppLifecycle
{
    private readonly ILogger<MainPageViewModel> log;

    private readonly IScreen screen;

    private readonly IDialog dialog;

    private readonly INotificationService notification;

    private readonly ITelemetryControl telemetryControl;

    private bool destroying;

    private bool foreground;

    public StartupState Startup { get; }

    public INavigator Navigator { get; }

    public NotificationValue<string> Title { get; } = new(string.Empty);

    public NotificationValue<bool> HeaderVisible { get; } = new();

    public NotificationValue<bool> FunctionVisible { get; } = new();

    public NotificationValue<Color?> StatusBarColor { get; } = new();

    public NotificationValue<StatusBarStyle> StatusBarStyle { get; } = new(CommunityToolkit.Maui.Core.StatusBarStyle.Default);

    public IReadOnlyList<FunctionState> Functions { get; } = [new(), new(), new(), new()];

    public FunctionState Function1 => Functions[0];
    public FunctionState Function2 => Functions[1];
    public FunctionState Function3 => Functions[2];
    public FunctionState Function4 => Functions[3];

    public IObserveCommand Function1Command { get; }
    public IObserveCommand Function2Command { get; }
    public IObserveCommand Function3Command { get; }
    public IObserveCommand Function4Command { get; }

    [ObservableProperty]
    public partial bool DiagnosticEnabled { get; set; }
    [ObservableProperty]
    public partial bool DiagnosticVisible { get; set; }

    public DiagnosticSampler DiagnosticSampler { get; }

    public IObserveCommand DiagnosticCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public MainPageViewModel(
        ILogger<MainPageViewModel> log,
        StartupState startup,
        INavigator navigator,
        IScreen screen,
        IDialog dialog,
        INotificationService notification,
        ITelemetryControl telemetryControl,
        DiagnosticSampler diagnosticSampler)
    {
        this.log = log;
        Startup = startup;
        Navigator = navigator;
        this.screen = screen;
        this.dialog = dialog;
        this.notification = notification;
        this.telemetryControl = telemetryControl;
        DiagnosticSampler = diagnosticSampler;

        Function1Command = CreateFunctionCommand(Functions[0], ShellEvent.Function1);
        Function2Command = CreateFunctionCommand(Functions[1], ShellEvent.Function2);
        Function3Command = CreateFunctionCommand(Functions[2], ShellEvent.Function3);
        Function4Command = CreateFunctionCommand(Functions[3], ShellEvent.Function4);

#if DEBUG
        DiagnosticEnabled = true;
#endif
        DiagnosticCommand = MakeDelegateCommand(ToggleDiagnostic);

        // Screen lock detection
        // ReSharper disable AsyncVoidLambda
        Disposables.Add(screen.StateChangedAsObservable().ObserveOnCurrentContext().Subscribe(async x =>
        {
            log.DebugScreenStateChanged(x.ScreenOn);
            if (x.ScreenOn)
            {
                await dialog.Toast("Screen on", true);
            }
        }));
        // ReSharper restore AsyncVoidLambda
    }

    private IObserveCommand CreateFunctionCommand(FunctionState function, ShellEvent shellEvent)
    {
        var command = MakeAsyncCommand(() => Navigator.NotifyAsync(shellEvent), () => function.Enabled.Value);
        return Observe(function.Enabled.AsObservable(nameof(NotificationValue<>.Value)), command);
    }

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    // ReSharper disable once AsyncVoidMethod
    public async void OnCreated()
    {
        screen.EnableDetectScreenState(true);
        telemetryControl.Suspend = false;
        foreground = true;

        await Startup.Completed;

        // Guard for the case where the Activity is recreated while initialization is still in progress
        if (destroying)
        {
            return;
        }

        Navigator.Exit();
        await Navigator.ForwardAsync(ViewId.Menu);

        // 通知のタップ (起動前に届いた分も含む) はどの画面でもトーストで示す。初期遷移の完了後に受け付ける
        // ReSharper disable AsyncVoidLambda
        Disposables.Add(notification.TappedAsObservable().ObserveOnCurrentContext().Subscribe(async x => await dialog.Toast(FormatNotificationTap(x), true)));
        // ReSharper restore AsyncVoidLambda
        if (notification.TakePendingTap() is { } pending)
        {
            await dialog.Toast(FormatNotificationTap(pending), true);
        }
    }

    private static string FormatNotificationTap(NotificationTappedEventArgs args) =>
        args.Action is null ? $"通知: {args.Payload}" : $"通知 [{args.Action}]: {args.Payload}";

    public void OnActivated()
    {
    }

    public void OnDeactivated()
    {
    }

    public void OnStopped()
    {
        telemetryControl.Suspend = true;
        foreground = false;
        UpdateSampler();
    }

    public void OnResumed()
    {
        telemetryControl.Suspend = false;
        foreground = true;
        UpdateSampler();
    }

    public void OnDestroying()
    {
        // The next window starts with the panel hidden
        foreground = false;
        UpdateSampler();
        telemetryControl.Suspend = true;

        destroying = true;
    }

    //--------------------------------------------------------------------------------
    // Diagnostic
    //--------------------------------------------------------------------------------

    private void ToggleDiagnostic()
    {
        DiagnosticVisible = !DiagnosticVisible;
        UpdateSampler();
    }

    private void UpdateSampler()
    {
        var running = DiagnosticSampler.IsRunning;
        if (DiagnosticVisible && foreground)
        {
            DiagnosticSampler.Start();
        }
        else
        {
            DiagnosticSampler.Stop();
        }

        if (running != DiagnosticSampler.IsRunning)
        {
            log.DebugSamplerChanged(DiagnosticSampler.IsRunning);
        }
    }
}
