namespace Template.MobileApp.Modules.Navigation.Stack;

using Template.MobileApp;

public class Stack1ViewModel : AppViewModelBase
{
    public Stack1ViewModel(ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.PushAsync(ViewId.NavigationStack2);
}
