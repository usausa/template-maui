namespace Template.MobileApp.Modules.Main;

using System.Diagnostics;

using Microsoft.Extensions.Options;

using Template.MobileApp.Components;
using Template.MobileApp.Helpers;
using Template.MobileApp.Services;

public sealed record LogFileInfo(string Name, long Size, DateTime Modified);

public sealed partial class DiagnosticsViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IShare share;

    private readonly DiagnosticLogProvider logProvider;

    private readonly ApiContext apiContext;

    private readonly DataService dataService;

    private readonly StartupState startup;

    private readonly Settings settings;

    private readonly string logDirectory;

    // Runtime
    [ObservableProperty]
    public partial long WorkingSet { get; private set; }

    [ObservableProperty]
    public partial long ManagedMemory { get; private set; }

    [ObservableProperty]
    public partial int ThreadCount { get; private set; }

    [ObservableProperty]
    public partial int Gc0Count { get; private set; }

    [ObservableProperty]
    public partial int Gc1Count { get; private set; }

    [ObservableProperty]
    public partial int Gc2Count { get; private set; }

    public int ProcessorCount { get; } = Environment.ProcessorCount;

    // Application

    public string ApplicationName { get; }

    public Version ApplicationVersion { get; }

    public string ApplicationBuild { get; }

    public string ApplicationPackageName { get; }

    public string Flavor { get; }

    public string DeviceName { get; }

    public Version DeviceVersion { get; }

    // Startup

    [ObservableProperty]
    public partial DateTime StartedAt { get; private set; }

    [ObservableProperty]
    public partial TimeSpan Uptime { get; private set; }

    [ObservableProperty]
    public partial TimeSpan? InitializationTime { get; private set; }

    // Connection

    [ObservableProperty]
    public partial bool ApiConfigured { get; private set; }

    [ObservableProperty]
    public partial bool GrpcConfigured { get; private set; }

    [ObservableProperty]
    public partial bool OtelConfigured { get; private set; }

    [ObservableProperty]
    public partial bool AIServiceConfigured { get; private set; }

    [ObservableProperty]
    public partial bool OllamaConfigured { get; private set; }

    [ObservableProperty]
    public partial bool ScpConfigured { get; private set; }

    [ObservableProperty]
    public partial bool IsAuthenticated { get; private set; }

    [ObservableProperty]
    public partial string LoginId { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? TokenExpires { get; private set; }

    public DeviceState DeviceState { get; }

    // Database

    [ObservableProperty]
    public partial DatabaseInfo Database { get; private set; } = new(string.Empty, 0, null, 0, 0, 0);

    // Log

    [ObservableProperty]
    public partial IReadOnlyList<LogFileInfo> LogFiles { get; private set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DiagnosticLogEntry> RecentLogs { get; private set; } = [];

    // Crash
    [ObservableProperty]
    public partial string? LastCrashReport { get; private set; }

    public IObserveCommand ShareLogsCommand { get; }

    public IObserveCommand ClearLogsCommand { get; }

    public IObserveCommand ClearCrashReportCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DiagnosticsViewModel(
        IDialog dialog,
        IShare share,
        IAppInfo appInfo,
        IDeviceInfo deviceInfo,
        IOptions<FileLoggerOptions> loggerOptions,
        DiagnosticLogProvider logProvider,
        ApiContext apiContext,
        DataService dataService,
        StartupState startup,
        Settings settings,
        DeviceState deviceState)
    {
        this.dialog = dialog;
        this.share = share;
        this.logProvider = logProvider;
        this.apiContext = apiContext;
        this.dataService = dataService;
        this.startup = startup;
        this.settings = settings;
        logDirectory = loggerOptions.Value.Directory ?? string.Empty;

        ApplicationName = appInfo.Name;
        ApplicationVersion = appInfo.Version;
        ApplicationBuild = appInfo.BuildString;
        ApplicationPackageName = appInfo.PackageName;
        Flavor = !String.IsNullOrEmpty(EmbeddedProperty.Flavor) ? EmbeddedProperty.Flavor : "Unknown";
        DeviceName = deviceInfo.Name;
        DeviceVersion = deviceInfo.Version;
        DeviceState = deviceState;

        ShareLogsCommand = MakeAsyncCommand(ShareLogsAsync, () => LogFiles.Count > 0);
        ClearLogsCommand = MakeDelegateCommand(ClearLogs, () => RecentLogs.Count > 0);
        ClearCrashReportCommand = MakeAsyncCommand(ClearCrashReportAsync, () => LastCrashReport is not null);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context) => LoadAsync();

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2() => LoadAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private async Task LoadAsync()
    {
        LoadRuntime();

        ApiConfigured = settings.IsApiConfigured();
        GrpcConfigured = settings.IsGrpcConfigured();
        OtelConfigured = settings.IsOtelConfigured();
        AIServiceConfigured = await settings.IsAIServiceConfiguredAsync();
        OllamaConfigured = settings.IsOllamaConfigured();
        ScpConfigured = settings.IsScpConfigured();
        IsAuthenticated = apiContext.IsAuthenticated;
        LoginId = apiContext.LoginId;
        TokenExpires = apiContext.TokenExpires;

        Database = await dataService.GetDatabaseInfoAsync();
        LogFiles = LoadLogFiles();
        RecentLogs = logProvider.GetEntries();
        LastCrashReport = CrashReport.GetLastReport();
    }

    private void LoadRuntime()
    {
        using var process = Process.GetCurrentProcess();
        var now = DateTime.Now;

        StartedAt = process.StartTime;
        Uptime = now - StartedAt;
        InitializationTime = startup.CompletedAt - StartedAt;
        WorkingSet = process.WorkingSet64;
        ManagedMemory = GC.GetTotalMemory(false);
        ThreadCount = process.Threads.Count;
        Gc0Count = GC.CollectionCount(0);
        Gc1Count = GC.CollectionCount(1);
        Gc2Count = GC.CollectionCount(2);
    }

    private List<LogFileInfo> LoadLogFiles()
    {
        if (!Directory.Exists(logDirectory))
        {
            return [];
        }

        return new DirectoryInfo(logDirectory).EnumerateFiles()
            .OrderByDescending(static x => x.Name)
            .Select(static x => new LogFileInfo(x.Name, x.Length, x.LastWriteTime))
            .ToList();
    }

    private Task ShareLogsAsync() =>
        share.RequestAsync(new ShareMultipleFilesRequest
        {
            Title = "Log files",
            Files = LogFiles.Select(x => new ShareFile(Path.Combine(logDirectory, x.Name))).ToList()
        });

    private void ClearLogs()
    {
        logProvider.Clear();
        RecentLogs = [];
    }

    private async Task ClearCrashReportAsync()
    {
        if (!await dialog.ConfirmAsync("Clear crash report ?"))
        {
            return;
        }

        CrashReport.ClearReport();
        LastCrashReport = null;
    }
}
