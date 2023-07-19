namespace Template.MobileApp.Modules.Basic;

public class BasicMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }

    public BasicMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
