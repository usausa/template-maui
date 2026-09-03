namespace Template.MobileApp.Modules.Basic;

public sealed partial class BasicBehaviorViewModel : AppViewModelBase
{
    public IObserveCommand FocusedCommand { get; }

    public IObserveCommand UnfocusedCommand { get; }

    [ObservableProperty]
    public partial string Focused { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Unfocused { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Phone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TypingText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LastStopped { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int SwitchCount { get; set; }

    public IObserveCommand TypingStoppedCommand { get; }

    public IObserveCommand SwitchToggledCommand { get; }

    public BasicBehaviorViewModel()
    {
        // [MEMO] Do not use individual focus control in real application.
        FocusedCommand = MakeDelegateCommand<string>(x => Focused = x);
        UnfocusedCommand = MakeDelegateCommand<string>(x => Unfocused = x);
        TypingStoppedCommand = MakeDelegateCommand(() => LastStopped = TypingText);
        SwitchToggledCommand = MakeDelegateCommand(() => SwitchCount++);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
