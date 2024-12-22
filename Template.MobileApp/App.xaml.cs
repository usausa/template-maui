namespace Template.MobileApp;

using Template.MobileApp.Helpers;
using Template.MobileApp.Modules;

public sealed partial class App
{
    private readonly INavigator navigator;

    public App(IServiceProvider serviceProvider, ILogger<App> log)
    {
        InitializeComponent();

        navigator = serviceProvider.GetRequiredService<INavigator>();
        MainPage = serviceProvider.GetRequiredService<MainPage>();

        // Start
        log.InfoApplicationStart(typeof(App).Assembly.GetName().Version, Environment.Version);
    }

    // ReSharper disable once AsyncVoidMethod
    protected override async void OnStart()
    {
        // Report previous exception
        await CrashReport.ShowReport();

        // Permissions
        await Permissions.RequestCameraAsync();
        await Permissions.RequestLocationAsync();

        await navigator.ForwardAsync(ViewId.Menu);
    }
}
