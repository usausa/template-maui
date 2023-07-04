namespace Template.MobileApp.Modules.Smart;

public class SmartMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }
    public ICommand BackCommand { get; }

    public SmartMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.Menu));
    }
}
