namespace Template.MobileApp;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

using Template.MobileApp.Helpers;
using Template.MobileApp.Services;

#pragma warning disable CA1724
public sealed partial class App
{
    private readonly IServiceProvider serviceProvider;

    private readonly ILogger<App> log;

    public App(IServiceProvider serviceProvider, ILogger<App> log)
    {
        this.serviceProvider = serviceProvider;
        this.log = log;

        // Light theme based application
        Current!.UserAppTheme = AppTheme.Light;

        InitializeComponent();
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

        // Initialize database
        var initializeError = await InitializeDataAsync();
        if (initializeError is not null)
        {
            var page = Current?.Windows[0].Page;
            if (page is not null)
            {
                await page.DisplayAlertAsync("Initialize error", $"Failed to initialize database.\r\n{initializeError.Message}", "Exit");
            }

            Current?.Quit();
            return;
        }

        // Start
        log.InfoApplicationStart(typeof(App).Assembly.GetName().Version, Environment.Version);

        // Completed
        serviceProvider.GetRequiredService<StartupState>().NotifyCompleted();
    }

    private async Task<Exception?> InitializeDataAsync()
    {
        try
        {
            await serviceProvider.GetRequiredService<DataService>().RebuildAsync();
            return null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SqliteException)
        {
            log.ErrorDatabaseInitializeFailed(ex);
            return ex;
        }
    }
}
#pragma warning restore CA1724
