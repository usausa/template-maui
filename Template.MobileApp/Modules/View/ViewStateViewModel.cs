namespace Template.MobileApp.Modules.View;

public sealed partial class ViewStateViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string? CurrentState { get; set; }

    [ObservableProperty]
    public partial bool PanelRequested { get; set; }

    public IObserveCommand StateCommand { get; }

    public IObserveCommand LoadPanelCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public ViewStateViewModel()
    {
        StateCommand = MakeDelegateCommand<string>(x => CurrentState = x);
        LoadPanelCommand = MakeDelegateCommand(() => PanelRequested = true);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
