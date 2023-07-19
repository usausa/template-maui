namespace Template.MobileApp.Modules.Basic;

public class BasicBehaviorViewModel : AppViewModelBase
{
    public BasicBehaviorViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
