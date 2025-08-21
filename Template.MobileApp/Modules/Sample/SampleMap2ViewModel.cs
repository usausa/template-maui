namespace Template.MobileApp.Modules.Sample;

public sealed class SampleMap2ViewModel : AppViewModelBase
{
    private const double InitialLatitude = 139.767052;
    private const double InitialLongitude = 35.681167;

    public MapsuiController Controller { get; } = new(InitialLatitude, InitialLongitude, 9);

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Controller.ZoomIn();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        Controller.ZoomOut();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        Controller.MoveTo(InitialLatitude, InitialLongitude);
        return Task.CompletedTask;
    }
}
