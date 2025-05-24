namespace Template.MobileApp.Modules.UI;

public sealed class UIMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public UIMenuViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
