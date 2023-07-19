namespace Template.MobileApp.Modules.Sample;

public class SampleMenuViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }

    public SampleMenuViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
