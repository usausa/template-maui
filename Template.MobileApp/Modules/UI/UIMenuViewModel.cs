namespace Template.MobileApp.Modules.UI;

public class UIMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }
    public ICommand BackCommand { get; }

    public UIMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.Menu));
    }
}
