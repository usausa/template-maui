namespace Template.MobileApp.Modules.Sample;

using Template.MobileApp;

public class SampleChatViewModel : AppViewModelBase
{
    public SampleChatViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}