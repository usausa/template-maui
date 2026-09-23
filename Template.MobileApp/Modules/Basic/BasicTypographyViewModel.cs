namespace Template.MobileApp.Modules.Basic;

public sealed class BasicTypographyViewModel : AppViewModelBase
{
    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
