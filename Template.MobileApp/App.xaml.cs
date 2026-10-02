namespace Template.MobileApp;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

using Template.MobileApp.Diagnostics;
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
        var window = new Window(serviceProvider.GetRequiredService<MainPage>());

        // 前面かどうかを Session に持つ
        var session = serviceProvider.GetRequiredService<Session>();
        window.Resumed += (_, _) => session.IsForeground = true;
        window.Stopped += (_, _) => session.IsForeground = false;

        return window;
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
            var dataService = serviceProvider.GetRequiredService<DataService>();
            await dataService.RebuildAsync();

            // Dummy data
            await PrepareDummyDataAsync(dataService);

            return null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SqliteException)
        {
            log.ErrorDatabaseInitializeFailed(ex);
            return ex;
        }
    }

    private static async ValueTask PrepareDummyDataAsync(DataService dataService)
    {
        await dataService.InsertWorkEnumerableAsync(
        [
            new WorkEntity { Id = 1, Name = "Sample-1" },
            new WorkEntity { Id = 2, Name = "Sample-2" },
            new WorkEntity { Id = 3, Name = "Sample-3" },
            new WorkEntity { Id = 4, Name = "Sample-4" }
        ]);

        var now = DateTime.Now;
        var today = now.Date;
        await dataService.InsertTodoEnumerableAsync(
        [
            new TodoEntity { Id = 1, Title = "請求書を送る", Note = "経理の担当へ PDF で送る", DueDate = today.AddDays(-2), IsImportant = true, CreatedAt = now, UpdatedAt = now },
            new TodoEntity { Id = 2, Title = "家賃の振り込み", Note = string.Empty, DueDate = today.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new TodoEntity { Id = 3, Title = "企画書のレビュー", Note = "3 章の図を差し替える", DueDate = today, IsImportant = true, CreatedAt = now, UpdatedAt = now },
            new TodoEntity { Id = 4, Title = "牛乳とパンを買う", Note = string.Empty, DueDate = today, CreatedAt = now, UpdatedAt = now },
            new TodoEntity { Id = 5, Title = "歯医者の予約", Note = "午前中に電話する", DueDate = today.AddDays(1), CreatedAt = now, UpdatedAt = now },
            new TodoEntity { Id = 6, Title = "週次レポートを書く", Note = string.Empty, DueDate = today.AddDays(4), CreatedAt = now, UpdatedAt = now },
            new TodoEntity { Id = 7, Title = "読みたい本をリストにする", Note = string.Empty, CreatedAt = now, UpdatedAt = now },
            new TodoEntity { Id = 8, Title = "部屋の掃除", Note = string.Empty, DueDate = today.AddDays(-1), IsDone = true, CreatedAt = now, UpdatedAt = now },
            new TodoEntity { Id = 9, Title = "ジムに行く", Note = "30 分のラン", DueDate = today, IsDone = true, CreatedAt = now, UpdatedAt = now }
        ]);
    }
}
#pragma warning restore CA1724
