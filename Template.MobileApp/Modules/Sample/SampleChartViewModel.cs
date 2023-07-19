namespace Template.MobileApp.Modules.Sample;

public class SampleChartViewModel : AppViewModelBase
{
    public SampleChartViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
