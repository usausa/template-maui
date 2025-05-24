namespace Template.MobileApp.Modules.UI;

public sealed class UICalendarViewModel : AppViewModelBase
{
    public UICalendarViewModel()
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
