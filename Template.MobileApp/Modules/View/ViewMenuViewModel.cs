namespace Template.MobileApp.Modules.View;

public sealed class ViewMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public ViewMenuViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
