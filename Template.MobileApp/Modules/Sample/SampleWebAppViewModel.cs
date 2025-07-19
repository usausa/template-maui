namespace Template.MobileApp.Modules.Sample;

public sealed class SampleWebAppViewModel : AppViewModelBase
{
    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
