namespace Template.MobileApp.Modules.Key;

using Template.MobileApp;

public class KeyMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }

    public KeyMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
