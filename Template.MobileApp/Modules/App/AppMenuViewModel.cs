namespace Template.MobileApp.Modules.App;

public sealed class AppMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public AppMenuViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
