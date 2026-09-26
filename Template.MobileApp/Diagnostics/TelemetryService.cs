namespace Template.MobileApp.Diagnostics;

using System.Diagnostics.Metrics;

using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Template.MobileApp.Components;

// ------------------------------------------------------------
// Control
// ------------------------------------------------------------

public interface ITelemetryControl
{
    bool Suspend { get; set; }

    string InstallationId { get; set; }

    Uri? EndPoint { get; set; }

    void Flush();
}

// ------------------------------------------------------------
// Status
// ------------------------------------------------------------

public sealed record TelemetrySendResult(DateTime Time, bool Succeeded, string? Reason);

public interface ITelemetryStatus
{
    bool IsActive { get; }

    string InstallationId { get; }

    Uri? EndPoint { get; }

    TelemetrySendResult? LastSend { get; }

    int ResendWaitingCount { get; }

    bool IsCrashPending();
}

// ------------------------------------------------------------
// TelemetryService
// ------------------------------------------------------------

public sealed partial class TelemetryService : ITelemetryControl, ITelemetryStatus, IDisposable
{
    // ------------------------------------------------------------
    // Const
    // ------------------------------------------------------------

    // Self category excluded from forwarding to avoid loops
    private const string SdkCategory = "OpenTelemetry.Sdk";

    private const string RuntimeMeterName = "System.Runtime";
    private const string HttpMeterName = "System.Net.Http";

    private const int ExportTimeout = 10_000;
    private const int MetricExportInterval = 30_000;
    private const int FlushTimeout = 3000;
    private const int CrashExportTimeout = 2000;
    private const int CrashFlushTimeout = 1000;

    private const int ResendCapacity = 60;

    // ------------------------------------------------------------
    // Field
    // ------------------------------------------------------------

    private static readonly string CategoryPrefix = DiagnosticsInstrumentation.Name;

    private static readonly string CrashCategory = typeof(CrashReport).FullName!;

    // System.Runtime instruments to be sent
    private static readonly string[] RuntimeInstruments =
    [
        "dotnet.gc.collections",
        "dotnet.gc.heap.total_allocated",
        "dotnet.exceptions"
    ];

    // System.Net.Http instruments to be sent
    private static readonly string[] HttpInstruments =
    [
        "http.client.request.duration"
    ];

    // SDK events of failed sends (logged by OnSent only when the state changes)
    private static readonly string[] SendFailureEvents =
    [
        "FailedToReachCollector",
        "ExportMethodException",
        "TransientHttpError",
        "HttpRequestFailed",
        "OperationUnexpectedlyCanceled",
        "RequestTimedOut",
        "ExportFailure"
    ];

    private readonly string instanceId = Guid.NewGuid().ToString();

    private readonly ILogger log;

    private readonly ILogger sdkLog;

    private readonly IAppInfo appInfo;

    private readonly IDeviceInfo deviceInfo;

    private readonly DeviceInformation deviceInformation;

    private readonly SdkEventListener sdkListener = new();

    private readonly string sentCrashPath;

    private Task queue = Task.CompletedTask;

    private Providers? providers;

    private int sendFailing;

    private bool disposed;

    // ------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------

    public TelemetryService(
        ILoggerFactory loggerFactory,
        IAppInfo appInfo,
        IDeviceInfo deviceInfo,
        IFileSystem fileSystem,
        DeviceInformation deviceInformation,
        TelemetryLoggerProvider loggerProvider)
    {
        log = loggerFactory.CreateLogger<TelemetryService>();
        sdkLog = loggerFactory.CreateLogger(SdkCategory);
        this.appInfo = appInfo;
        this.deviceInfo = deviceInfo;
        this.deviceInformation = deviceInformation;
        sentCrashPath = Path.Combine(fileSystem.AppDataDirectory, "telemetry-crash-sent.txt");

        sdkListener.Written += OnSdkEventWritten;
        CrashReport.Crashed += OnCrashed;

        loggerProvider.Attach(this);

        // Suspended until the application comes to the foreground
        Suspend = true;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        CrashReport.Crashed -= OnCrashed;

        sdkListener.Written -= OnSdkEventWritten;
        sdkListener.Dispose();

        providers?.Dispose();
        providers = null;
    }

    // ------------------------------------------------------------
    // Control
    // ------------------------------------------------------------

    public bool Suspend
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;

            if (value)
            {
                Flush();
            }
        }
    }

    public string InstallationId { get; set; } = string.Empty;

    public Uri? EndPoint
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            log.InfoTelemetryEndPointChanged(value);

            if (value is null)
            {
                Enqueue(StopProviders);
            }
            else
            {
                Enqueue(() =>
                {
                    StartProviders(value);
                    SendPendingCrash();
                });
            }
        }
    }

    public void Flush()
    {
        if (IsActive)
        {
            Enqueue(() => Volatile.Read(ref providers)?.Flush(FlushTimeout));
        }
    }

    // ------------------------------------------------------------
    // Status
    // ------------------------------------------------------------

    public bool IsActive => EndPoint is not null;

    public TelemetrySendResult? LastSend { get; private set; }

    public int ResendWaitingCount => Volatile.Read(ref providers)?.ResendWaitingCount ?? 0;

    public bool IsCrashPending() =>
        (CrashReport.GetLastReport() is { } info) && (info.Id != LoadSentCrashId());

    // ------------------------------------------------------------
    // Log
    // ------------------------------------------------------------

    internal bool IsLogEnabled(string category, LogLevel logLevel) =>
        (logLevel >= LogLevel.Warning) && (logLevel != LogLevel.None) && IsForwardedCategory(category) && (Volatile.Read(ref providers) is not null);

    internal IDisposable? BeginLogScope<TState>(string category, TState state)
        where TState : notnull =>
        IsForwardedCategory(category) ? Volatile.Read(ref providers)?.CreateLogger(category)?.BeginScope(state) : null;

    internal void WriteLog<TState>(string category, LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsLogEnabled(category, logLevel))
        {
            Volatile.Read(ref providers)?.CreateLogger(category)?.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    // ------------------------------------------------------------
    // Telemetry
    // ------------------------------------------------------------

    private void Enqueue(Action action)
    {
        queue = queue.ContinueWith(
            _ =>
            {
                try
                {
                    action();
                }
                catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or NotSupportedException or ObjectDisposedException or IOException or UnauthorizedAccessException)
                {
                    log.WarnTelemetryOperationFailed(ex);
                }
            },
            CancellationToken.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);
    }

    private void StartProviders(Uri endPoint)
    {
        StopProviders();

        providers = new Providers(CreateResource(), endPoint, OnSent);
    }

    private void StopProviders()
    {
        Interlocked.Exchange(ref providers, null)?.Dispose();
    }

    private void SendPendingCrash()
    {
        var current = Volatile.Read(ref providers);
        if ((current is not null) && (CrashReport.GetLastReport() is { } info) && (info.Id != LoadSentCrashId()) && current.SendCrash(info))
        {
            SaveSentCrashId(info.Id);
        }
    }

    // ------------------------------------------------------------
    // Event
    // ------------------------------------------------------------

    private void OnCrashed(object? sender, CrashEventArgs e)
    {
        var current = Volatile.Read(ref providers);
        if (current is null)
        {
            return;
        }

        if (current.SendCrash(e.Info))
        {
            SaveSentCrashId(e.Info.Id);
        }

        if (e.IsTerminating)
        {
            current.ShutdownLogs(CrashFlushTimeout);
        }
    }

    private void OnSdkEventWritten(object? sender, SdkEventArgs e)
    {
        if (!SendFailureEvents.Contains(e.Name))
        {
            sdkLog.WarnTelemetrySdkEvent(e.Source, e.Level, e.Message);
        }
    }

    private void OnSent(bool succeeded, string? reason)
    {
        LastSend = new TelemetrySendResult(DateTime.Now, succeeded, reason);

        var failing = succeeded ? 0 : 1;
        if (Interlocked.Exchange(ref sendFailing, failing) == failing)
        {
            return;
        }

        if (succeeded)
        {
            log.InfoTelemetrySendRecovered();
        }
        else
        {
            log.WarnTelemetrySendFailed(reason);
        }
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    private string? LoadSentCrashId() => File.Exists(sentCrashPath) ? File.ReadAllText(sentCrashPath) : null;

    private void SaveSentCrashId(string id) => File.WriteAllText(sentCrashPath, id);

    private static bool IsForwardedCategory(string category) =>
        category.StartsWith(CategoryPrefix, StringComparison.Ordinal);

    private ResourceBuilder CreateResource() =>
        ResourceBuilder.CreateEmpty()
            .AddService(DiagnosticsInstrumentation.Name, serviceVersion: appInfo.VersionString, autoGenerateServiceInstanceId: false, serviceInstanceId: instanceId)
            .AddTelemetrySdk()
            .AddAttributes(new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["device.id"] = deviceInformation.DeviceId,
                ["app.installation.id"] = InstallationId,
                ["device.manufacturer"] = deviceInfo.Manufacturer,
                ["device.model.identifier"] = deviceInfo.Model,
                ["os.type"] = "linux",
                ["os.name"] = deviceInfo.Platform.ToString(),
                ["os.version"] = deviceInfo.VersionString
            });

    private static void ConfigureExporter(OtlpExporterOptions options, Uri endPoint, string path, Func<HttpClient> httpClientFactory)
    {
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
        options.Endpoint = new Uri(endPoint, path);
        options.TimeoutMilliseconds = ExportTimeout;
        options.HttpClientFactory = httpClientFactory;
    }

    private static partial HttpMessageHandler CreateExportHandler();

    private static MetricStreamConfiguration? SelectInstrument(Instrument instrument) =>
        instrument.Meter.Name switch
        {
            RuntimeMeterName => RuntimeInstruments.Contains(instrument.Name) ? null : MetricStreamConfiguration.Drop,
            HttpMeterName => HttpInstruments.Contains(instrument.Name) ? null : MetricStreamConfiguration.Drop,
            _ => null
        };

    // ------------------------------------------------------------
    // Provider
    // ------------------------------------------------------------

    private sealed class Providers : IDisposable
    {
        private readonly TracerProvider tracerProvider;

        private readonly MeterProvider meterProvider;

        private readonly ServiceProvider logServices;

        private readonly ILoggerFactory crashLoggerFactory;

        private TelemetrySendHandler? sendHandler;

        private TelemetrySendHandler? crashSendHandler;

        public int ResendWaitingCount => Volatile.Read(ref sendHandler)?.WaitingCount ?? 0;

        public Providers(ResourceBuilder resource, Uri endPoint, Action<bool, string?> sent)
        {
            // Trace
            tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resource)
                .AddSource(DiagnosticsInstrumentation.Name)
                .AddOtlpExporter(x => ConfigureExporter(x, endPoint, "v1/traces", CreateHttpClient))
                .Build();

            // Metrics
            meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resource)
                .AddMeter(DiagnosticsInstrumentation.Name, RuntimeMeterName, HttpMeterName)
                .AddView(SelectInstrument)
                .AddOtlpExporter((exporter, reader) =>
                {
                    ConfigureExporter(exporter, endPoint, "v1/metrics", CreateHttpClient);
                    reader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = MetricExportInterval;
                    reader.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                })
                .Build();

            // Logs
            var services = new ServiceCollection();
            services.AddLogging(logging => logging
                .SetMinimumLevel(LogLevel.Warning)
                .AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resource);
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                    options.AddOtlpExporter(x => ConfigureExporter(x, endPoint, "v1/logs", CreateHttpClient));
                }));
            logServices = services.BuildServiceProvider();

            // Crash logs
            crashLoggerFactory = LoggerFactory.Create(logging => logging
                .AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resource);
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                    options.AddProcessor(_ => new SimpleLogRecordExportProcessor(new OtlpLogExporter(new OtlpExporterOptions
                    {
                        Protocol = OtlpExportProtocol.HttpProtobuf,
                        Endpoint = new Uri(endPoint, "v1/logs"),
                        TimeoutMilliseconds = CrashExportTimeout,
                        HttpClientFactory = () => new HttpClient(crashSendHandler = new TelemetrySendHandler(CreateExportHandler(), 0, null)) { Timeout = TimeSpan.FromMilliseconds(CrashExportTimeout) }
                    })));
                }));

            return;

            HttpClient CreateHttpClient() => new(sendHandler ??= new TelemetrySendHandler(CreateExportHandler(), ResendCapacity, sent), false) { Timeout = TimeSpan.FromMilliseconds(ExportTimeout) };
        }

        public void Dispose()
        {
            crashLoggerFactory.Dispose();
            logServices.Dispose();
            meterProvider.Dispose();
            tracerProvider.Dispose();
            sendHandler?.Dispose();
        }

        public ILogger? CreateLogger(string category)
        {
            try
            {
                return logServices.GetRequiredService<ILoggerFactory>().CreateLogger(category);
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        public void Flush(int timeout)
        {
            tracerProvider.ForceFlush(timeout);
            meterProvider.ForceFlush(timeout);
            logServices.GetRequiredService<LoggerProvider>().ForceFlush(timeout);
        }

        public bool ShutdownLogs(int timeout) => logServices.GetRequiredService<LoggerProvider>().Shutdown(timeout);

        public bool SendCrash(CrashInfo info)
        {
            ILogger logger;
            try
            {
                logger = crashLoggerFactory.CreateLogger(CrashCategory);
            }
            catch (ObjectDisposedException)
            {
                return false;
            }

            var succeeded = Volatile.Read(ref crashSendHandler)?.SucceededCount ?? 0;
            using (logger.BeginScope(new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["app.crash.id"] = info.Id,
                ["exception.type"] = info.ExceptionType,
                ["exception.message"] = info.Message,
                ["exception.stacktrace"] = info.Detail
            }))
            {
                logger.CriticalApplicationCrashed(info.Time);
            }

            return (Volatile.Read(ref crashSendHandler)?.SucceededCount ?? 0) > succeeded;
        }
    }
}
