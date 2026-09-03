namespace Template.MobileApp.Modules.View;

public sealed partial class ViewStateViewModel : AppViewModelBase
{
    // StateContainer の状態キー (空文字は既定コンテンツ = Success 表示)
    [ObservableProperty]
    public partial string CurrentState { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool PanelRequested { get; set; }

    public IObserveCommand StateCommand { get; }

    public IObserveCommand LoadPanelCommand { get; }

    public ViewStateViewModel()
    {
        StateCommand = MakeDelegateCommand<string>(x => CurrentState = x);
        LoadPanelCommand = MakeDelegateCommand(() => PanelRequested = true);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
