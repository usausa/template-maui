namespace Template.MobileApp.Modules.Sample;

public sealed class SampleMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public SampleMenuViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
