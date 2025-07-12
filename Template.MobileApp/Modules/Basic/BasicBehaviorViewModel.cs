namespace Template.MobileApp.Modules.Basic;

public sealed partial class BasicBehaviorViewModel : AppViewModelBase
{
    public IObserveCommand FocusedCommand { get; }

    public IObserveCommand UnfocusedCommand { get; }

    [ObservableProperty]
    public partial string Focused { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Unfocused { get; set; } = string.Empty;

    public BasicBehaviorViewModel()
    {
        // [MEMO] Do not use individual focus control in real application.
        FocusedCommand = MakeDelegateCommand<string>(x => Focused = x);
        UnfocusedCommand = MakeDelegateCommand<string>(x => Unfocused = x);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
