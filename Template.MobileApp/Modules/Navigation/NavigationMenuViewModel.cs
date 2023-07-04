namespace Template.MobileApp.Modules.Navigation;

public class NavigationMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }
    public ICommand SharedCommand { get; }

    public NavigationMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
        SharedCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(ViewId.NavigationSharedInput, Parameters.MakeNextViewId(x)));
    }
}
