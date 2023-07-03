namespace Template.MobileApp.Modules.Navigation;

public class NavigationMenuViewModel : AppViewModelBase
{
    public ICommand BackCommand { get; }

    public NavigationMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.Menu));
    }
}
