namespace Template.MobileApp;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

using Smart.Mvvm.Resolver;

using Template.MobileApp.Services;

public sealed class ApplicationInitializer : IMauiInitializeService
{
    // 非同期初期化のTask。App.OnStartが画面遷移前に完了を待つ
    public Task StartupTask { get; private set; } = Task.CompletedTask;

    // DB初期化に失敗した場合の例外。App.OnStartが原因を提示してから終了する
    public Exception? InitializeError { get; private set; }

    public void Initialize(IServiceProvider services)
    {
        try
        {
            InitializeCore(services);
        }
        catch (Exception ex)
        {
            // Release (トリミング有効) では例外メッセージがリソースキー化され原因が追えなくなるため、
            // 起動失敗時は型と内部例外の連鎖を logcat へ完全出力してから落とす
            // (Console 出力は Release では logcat に転送されないため Android の Log を直接使う)
#if ANDROID
            Android.Util.Log.Error("StartupError", ex.ToString());
#else
            Console.WriteLine($"[StartupError] {ex}");
#endif
            throw;
        }
    }

    private void InitializeCore(IServiceProvider services)
    {
        // Setup provider
        ResolveProvider.Default.Provider = services;

#if DEBUG
        // 生成ファクトリで解決できずリフレクションへフォールバックした型を確認する
        // (出力を GeneratedFactory.cs に貼り付けて生成対象に加える)
        if (services is BunnyTail.DependencyInjection.GeneratedServiceProvider generatedProvider)
        {
            foreach (var line in BunnyTail.DependencyInjection.Diagnostics.ServiceFactoryReportExtensions.DescribeRuntimeFallbacks(generatedProvider).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                System.Diagnostics.Debug.WriteLine(line);
            }
        }
#endif

        var settings = services.GetRequiredService<Settings>();

        // Initial setting
        if (String.IsNullOrEmpty(settings.ApiEndPoint) && !String.IsNullOrEmpty(EmbeddedProperty.ApiEndPoint))
        {
            settings.ApiEndPoint = EmbeddedProperty.ApiEndPoint;
        }

        // Setup navigator
        var navigator = services.GetRequiredService<INavigator>();
        navigator.Navigated += (_, args) =>
        {
            // for debug
            System.Diagnostics.Debug.WriteLine(
                $"Navigated: [{args.Context.FromId}]->[{args.Context.ToId}] : stacked=[{navigator.StackedCount}]");
        };

        // Setting
        if (String.IsNullOrEmpty(settings.UniqueId))
        {
            var uniqueId = Guid.NewGuid();
            settings.UniqueId = uniqueId.ToString();
        }

        var apiContext = services.GetRequiredService<ApiContext>();
        if (!String.IsNullOrEmpty(settings.ApiEndPoint))
        {
            apiContext.BaseAddress = new Uri(settings.ApiEndPoint);
        }

        // Service
        StartupTask = InitializeAsync(services);
    }

    private async Task InitializeAsync(IServiceProvider services)
    {
        try
        {
            var dataService = services.GetRequiredService<DataService>();
            await dataService.RebuildAsync();
        }
        // ファイルロック・権限・DB破損は発生が予期される失敗のため個別に捕捉する
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SqliteException)
        {
            InitializeError = ex;
            services.GetRequiredService<ILogger<ApplicationInitializer>>().ErrorDatabaseInitializeFailed(ex);
        }
    }
}
