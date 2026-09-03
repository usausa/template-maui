namespace Template.MobileApp;

using Template.MobileApp.Shell;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class MainPageViewModel : ExtendViewModelBase, IShellControl, IAppLifecycle
{
    private readonly IScreen screen;

    public INavigator Navigator { get; }

    public NotificationValue<string> Title { get; } = new(string.Empty);

    // 初回ナビゲーションでShellPropertyから反映されるまでは空ヘッダーを出さない
    public NotificationValue<bool> HeaderVisible { get; } = new();

    public NotificationValue<bool> FunctionVisible { get; } = new();

    public IReadOnlyList<FunctionState> Functions { get; } = [new(), new(), new(), new()];

    // XAML用エイリアス (compiled bindingでのインデクサ利用を避ける)
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

    public IObserveCommand DiagnosticCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public MainPageViewModel(
        ILogger<MainPageViewModel> log,
        INavigator navigator,
        IScreen screen,
        IDialog dialog)
    {
        Navigator = navigator;
        this.screen = screen;

        Function1Command = CreateFunctionCommand(Functions[0], ShellEvent.Function1);
        Function2Command = CreateFunctionCommand(Functions[1], ShellEvent.Function2);
        Function3Command = CreateFunctionCommand(Functions[2], ShellEvent.Function3);
        Function4Command = CreateFunctionCommand(Functions[3], ShellEvent.Function4);

#if DEBUG
        DiagnosticEnabled = true;
#endif
        DiagnosticCommand = MakeDelegateCommand(() => DiagnosticVisible = !DiagnosticVisible);

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

        // EnabledはVMと別のオブジェクトのため、CanExecuteの再評価を明示的に接続する
        Disposables.Add(function.Enabled.AsObservable(nameof(NotificationValue<bool>.Value))
            .Subscribe(_ => command.RaiseCanExecuteChanged()));

        return command;
    }

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    public void OnCreated()
    {
        screen.EnableDetectScreenState(true);
    }

    public void OnActivated()
    {
    }

    public void OnDeactivated()
    {
    }

    public void OnStopped()
    {
    }

    public void OnResumed()
    {
    }

    public void OnDestroying()
    {
    }
}
