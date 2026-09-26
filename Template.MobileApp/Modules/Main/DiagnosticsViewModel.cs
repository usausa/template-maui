namespace Template.MobileApp.Modules.Main;

using System.Globalization;

using Microsoft.Extensions.Options;

using Template.MobileApp.Components;
using Template.MobileApp.Diagnostics;
using Template.MobileApp.Services;

public sealed record LogFileInfo(string Name, long Size, DateTime Modified);

public sealed partial class DiagnosticsViewModel : AppViewModelBase
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(1);

    private readonly IDialog dialog;

    private readonly IShare share;

    private readonly IDispatcherTimer refreshTimer;

    private readonly DiagnosticLogProvider logProvider;

    private readonly ITelemetryStatus telemetryStatus;

    private readonly ApiContext apiContext;

    private readonly DataService dataService;

    private readonly StartupState startup;

    private readonly string logDirectory;

    private readonly string logPrefix;

    // Device

    public string DeviceName { get; }

    public Version DeviceVersion { get; }

    public int ProcessorCount { get; } = Environment.ProcessorCount;

    public string DeviceId { get; }

    // Application

    public string InstallationId { get; }

    // Startup

    public DateTime StartedAt { get; }

    [ObservableProperty]
    public partial TimeSpan Uptime { get; private set; }

    [ObservableProperty]
    public partial TimeSpan? InitializationTime { get; private set; }

    // Database

    [ObservableProperty]
    public partial DatabaseInfo Database { get; private set; } = new(string.Empty, 0, null, 0, 0, 0);

    // Connection

    [ObservableProperty]
    public partial bool IsAuthenticated { get; private set; }

    [ObservableProperty]
    public partial string LoginId { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? TokenExpires { get; private set; }

    public DeviceState DeviceState { get; }

    // Telemetry

    [ObservableProperty]
    public partial bool TelemetryActive { get; private set; }

    [ObservableProperty]
    public partial TelemetrySendResult? LastSend { get; private set; }

    [ObservableProperty]
    public partial int ResendWaitingCount { get; private set; }

    [ObservableProperty]
    public partial bool CrashPending { get; private set; }

    // Log

    [ObservableProperty]
    public partial IReadOnlyList<LogFileInfo> LogFiles { get; private set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DiagnosticLogEntry> RecentLogs { get; private set; } = [];

    // Crash

    [ObservableProperty]
    public partial string? LastCrashReport { get; private set; }

    // Command

    public IObserveCommand ShareLogsCommand { get; }

    public IObserveCommand DeleteLogsCommand { get; }

    public IObserveCommand ClearCrashReportCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DiagnosticsViewModel(
        IDialog dialog,
        IShare share,
        IDeviceInfo deviceInfo,
        IDispatcher dispatcher,
        IOptions<FileLoggerOptions> loggerOptions,
        DiagnosticLogProvider logProvider,
        ITelemetryStatus telemetryStatus,
        ApiContext apiContext,
        DataService dataService,
        DeviceInformation deviceInformation,
        StartupState startup,
        DeviceState deviceState)
    {
        this.dialog = dialog;
        this.share = share;
        this.logProvider = logProvider;
        this.telemetryStatus = telemetryStatus;
        this.apiContext = apiContext;
        this.dataService = dataService;
        this.startup = startup;
        logDirectory = loggerOptions.Value.Directory ?? string.Empty;
        logPrefix = loggerOptions.Value.Prefix ?? string.Empty;

        DeviceName = deviceInfo.Name;
        DeviceVersion = deviceInfo.Version;
        DeviceId = deviceInformation.DeviceId;
        InstallationId = telemetryStatus.InstallationId;
        StartedAt = deviceInformation.StartTime;
        DeviceState = deviceState;

        ShareLogsCommand = MakeAsyncCommand(ShareLogsAsync, () => LogFiles.Count > 0);
        DeleteLogsCommand = MakeAsyncCommand(DeleteLogsAsync, () => (RecentLogs.Count > 0) || EnumerateOldLogFiles().Any());
        ClearCrashReportCommand = MakeAsyncCommand(ClearCrashReportAsync, () => LastCrashReport is not null);

        refreshTimer = dispatcher.CreateTimer();
        refreshTimer.Interval = RefreshInterval;
        Disposables.Add(refreshTimer.TickAsObservable().Subscribe(_ => UpdateStatus()));
        Disposables.Add(new DelegateDisposable(refreshTimer.Stop));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context) => LoadAsync();

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        refreshTimer.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        refreshTimer.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private async Task LoadAsync()
    {
        InitializationTime = startup.CompletedAt - StartedAt;
        UpdateStatus();

        Database = await dataService.GetDatabaseInfoAsync();

        IsAuthenticated = apiContext.IsAuthenticated;
        LoginId = apiContext.LoginId;
        TokenExpires = apiContext.TokenExpires;

        CrashPending = telemetryStatus.IsCrashPending();

        LogFiles = LoadLogFiles();
        RecentLogs = logProvider.GetEntries();

        var crash = CrashReport.GetLastReport();
        LastCrashReport = crash?.ToReport();
    }

    private void UpdateStatus()
    {
        Uptime = DateTime.Now - StartedAt;
        TelemetryActive = telemetryStatus.IsActive;
        LastSend = telemetryStatus.LastSend;
        ResendWaitingCount = telemetryStatus.ResendWaitingCount;
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

    private async Task DeleteLogsAsync()
    {
        if (!await dialog.ConfirmAsync("Delete old log files ?"))
        {
            return;
        }

        foreach (var file in EnumerateOldLogFiles())
        {
            try
            {
                File.Delete(Path.Combine(logDirectory, file.Name));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Left in the list
            }
        }

        logProvider.Clear();
        LogFiles = LoadLogFiles();
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
        CrashPending = false;
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private IEnumerable<LogFileInfo> EnumerateOldLogFiles()
    {
        var current = String.Concat(logPrefix, DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture), ".log");
        return LogFiles.Where(x => x.Name != current);
    }
}
