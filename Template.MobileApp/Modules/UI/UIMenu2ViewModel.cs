namespace Template.MobileApp.Modules.UI;

public sealed class UIMenu2ViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public UIMenu2ViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(ViewId.UIMenu1);
}
