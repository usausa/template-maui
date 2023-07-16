namespace Template.MobileApp.Modules.Basic;

using Template.MobileApp;

public class BasicStyleViewModel : AppViewModelBase
{
    public BasicStyleViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
