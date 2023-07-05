namespace Template.MobileApp.Modules.Smart;

public class SmartMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }

    public SmartMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
