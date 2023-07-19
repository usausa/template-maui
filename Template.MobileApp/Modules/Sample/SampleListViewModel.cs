namespace Template.MobileApp.Modules.Sample;

public class SampleListViewModel : AppViewModelBase
{
    public ICommand ForwardCommand { get; }

    public SampleListViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
