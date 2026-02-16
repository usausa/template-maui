namespace Template.MobileApp;

using Microsoft.Extensions.DependencyInjection;

using Template.MobileApp.Helpers;
using Template.MobileApp.Modules;

public sealed partial class App
{
    private readonly IServiceProvider serviceProvider;

    public App(IServiceProvider serviceProvider, ILogger<App> log)
    {
        this.serviceProvider = serviceProvider;

        // Light theme based application
        Current!.UserAppTheme = AppTheme.Light;

        InitializeComponent();

        // Start
#pragma warning disable CA1873
        log.InfoApplicationStart(typeof(App).Assembly.GetName().Version, Environment.Version);
#pragma warning restore CA1873
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(serviceProvider.GetRequiredService<MainPage>());
    }

    // ReSharper disable once AsyncVoidMethod
    protected override async void OnStart()
    {
        // Report previous exception
        await CrashReport.ShowReport();

        // Permissions
        await Permissions.RequestCameraAsync();
        await Permissions.RequestMicrophoneAsync();
        await Permissions.RequestLocationAsync();

        // Navigate
        var navigator = serviceProvider.GetRequiredService<INavigator>();
        await navigator.ForwardAsync(ViewId.Menu);
    }
}
