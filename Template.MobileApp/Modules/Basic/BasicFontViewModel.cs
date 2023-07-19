namespace Template.MobileApp.Modules.Basic;

public class BasicFontViewModel : AppViewModelBase
{
    public BasicFontViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
