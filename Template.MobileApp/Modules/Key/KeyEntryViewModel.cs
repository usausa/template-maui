namespace Template.MobileApp.Modules.Key;

public class KeyEntryViewModel : AppViewModelBase
{
    public ICommand BackCommand { get; }

    public KeyEntryViewModel(
        ApplicationState applicationState)
        : base(applicationState)
    {
        BackCommand = MakeAsyncCommand(async () => await Navigator.ForwardAsync(ViewId.DeviceMenu));
    }
}
