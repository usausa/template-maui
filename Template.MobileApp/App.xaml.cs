namespace Template.MobileApp;

using Microsoft.Extensions.DependencyInjection;

using Template.MobileApp.Helpers;
using Template.MobileApp.Modules;

#pragma warning disable CA1724
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
        log.InfoApplicationStart(typeof(App).Assembly.GetName().Version, Environment.Version);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(serviceProvider.GetRequiredService<MainPage>());
    }

    // ReSharper disable once AsyncVoidMethod
    // 例外時はグローバルハンドラ経由でCrashReportに記録されfail-fastとなる (次回起動時に表示)
    protected override async void OnStart()
    {
        // Report previous exception
        await CrashReport.ShowReport();

        // 権限要求は起動時に一括では行わず、各機能の利用画面側でCheck→Requestする

        // 非同期初期化(DB再構築)の完了を待ってから画面遷移する
        var initializer = serviceProvider.GetRequiredService<ApplicationInitializer>();
        await initializer.StartupTask;

        // DBを作れない環境では以降の画面が成立しないため、原因を提示して終了する
        // (無言でクラッシュすると次回起動でも同じ所で落ち、理由が分からないまま復帰できないため)
        if (initializer.InitializeError is not null)
        {
            var page = Current?.Windows[0].Page;
            if (page is not null)
            {
                await page.DisplayAlertAsync(
                    "Initialize error",
                    $"Failed to initialize database.\r\n{initializer.InitializeError.Message}",
                    "Exit");
            }

            Current?.Quit();
            return;
        }

        // Navigate
        var navigator = serviceProvider.GetRequiredService<INavigator>();
        await navigator.ForwardAsync(ViewId.Menu);
    }
}
#pragma warning restore CA1724
