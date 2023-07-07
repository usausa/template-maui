namespace Template.MobileApp.Modules.Main;

using Template.MobileApp;

public class MenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }

    public MenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }
}
