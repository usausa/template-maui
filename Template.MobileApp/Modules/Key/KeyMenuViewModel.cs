namespace Template.MobileApp.Modules.Key;

public class KeyMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }
    public ICommand BackCommand { get; }

    public KeyMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.Menu));
    }
}
