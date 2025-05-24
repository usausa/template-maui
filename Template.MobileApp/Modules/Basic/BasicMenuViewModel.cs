namespace Template.MobileApp.Modules.Basic;

public sealed class BasicMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public BasicMenuViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
