namespace Template.MobileApp.Modules.View;

public sealed class ViewBorderViewModel : AppViewModelBase
{
    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
