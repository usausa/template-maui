namespace Template.MobileApp.Modules.Key;

public class KeyMiscViewModel : AppViewModelBase
{
    public ICommand BackCommand { get; }

    public KeyMiscViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.DeviceMenu));
    }
}
