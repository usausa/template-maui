namespace Template.MobileApp.Modules.Key;

public class KeyListViewModel : AppViewModelBase
{
    public ICommand BackCommand { get; }

    public KeyListViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.DeviceMenu));
    }
}
