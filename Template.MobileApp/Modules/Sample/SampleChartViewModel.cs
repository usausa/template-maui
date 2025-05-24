namespace Template.MobileApp.Modules.Sample;

public sealed class SampleChartViewModel : AppViewModelBase
{
    public SampleChartViewModel()
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
