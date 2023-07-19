namespace Template.MobileApp.Modules.Main;

public class MenuViewModel : AppViewModelBase
{
    public NotificationValue<string> Flavor { get; } = new();

    public NotificationValue<Version> Version { get; } = new();

    public ICommand ForwardCommand { get; }

    public MenuViewModel(
        ApplicationState applicationState,
        IAppInfo appInfo)
        : base(applicationState)
    {
        Flavor.Value = !String.IsNullOrEmpty(Variants.Flavor()) ? Variants.Flavor() : "Unknown";
        Version.Value = appInfo.Version;

        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }
}
