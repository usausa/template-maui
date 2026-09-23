namespace Template.MobileApp.Modules.Control;

public sealed class ControlMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public ControlMenuViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
