namespace Template.MobileApp.Modules.Sample;

public sealed class SampleListViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public SampleListViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
