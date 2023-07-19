namespace Template.MobileApp.Modules.Basic;

public class BasicValidationViewModel : AppViewModelBase
{
    public BasicValidationViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
