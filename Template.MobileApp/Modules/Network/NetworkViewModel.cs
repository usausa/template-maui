namespace Template.MobileApp.Modules.Network;

public class NetworkViewModel : AppViewModelBase
{
    public ICommand BackCommand { get; }

    public NetworkViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.Menu));
    }
}
