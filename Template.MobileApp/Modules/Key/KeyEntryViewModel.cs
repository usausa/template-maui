namespace Template.MobileApp.Modules.Key;

public class KeyEntryViewModel : AppViewModelBase
{
    public KeyEntryViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.KeyMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
