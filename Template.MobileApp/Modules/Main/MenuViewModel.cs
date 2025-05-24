namespace Template.MobileApp.Modules.Main;

public sealed partial class MenuViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string Flavor { get; set; }

    [ObservableProperty]
    public partial Version Version { get; set; }

    public IObserveCommand ForwardCommand { get; }

    public MenuViewModel(IAppInfo appInfo)
    {
        Flavor = !String.IsNullOrEmpty(Variants.Flavor) ? Variants.Flavor : "Unknown";
        Version = appInfo.Version;

        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }
}
