namespace Template.MobileApp.Modules.Main;

using BarcodeScanning;

using Template.MobileApp.Diagnostics;
using Template.MobileApp.Helpers;
using Template.MobileApp.Services;

public sealed partial class SettingViewModel : AppViewModelBase
{
    private static readonly TimeSpan DetectInterval = TimeSpan.FromSeconds(3);

    private readonly Settings settings;

    public BarcodeController Controller { get; } = new();

    [ObservableProperty]
    public partial string? ApiEndPoint { get; set; }

    [ObservableProperty]
    public partial string? GrpcEndPoint { get; set; }

    [ObservableProperty]
    public partial string? OtelEndPoint { get; set; }

    [ObservableProperty]
    public partial bool TelemetryEnabled { get; set; }

    [ObservableProperty]
    public partial string? AIServiceEndPoint { get; set; }

    [ObservableProperty]
    public partial string? AIServiceKey { get; set; }

    [ObservableProperty]
    public partial string? OllamaEndPoint { get; set; }

    [ObservableProperty]
    public partial string? OllamaModel { get; set; }

    [ObservableProperty]
    public partial string? SshHost { get; set; }

    [ObservableProperty]
    public partial string? SshUser { get; set; }

    [ObservableProperty]
    public partial string? SshPassword { get; set; }

    public IObserveCommand DetectCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SettingViewModel(
        ApiContext apiContext,
        ITelemetryControl telemetryControl,
        Settings settings)
    {
        this.settings = settings;

        Controller.AimMode = true;
        Controller.VibrationOnDetect = true;
        Controller.CaptureNextFrame = false;

        SubscribeTelemetryEnabled(x =>
        {
            settings.TelemetryEnabled = x;
            telemetryControl.EndPoint = settings.GetTelemetryEndPoint();
        });

        DetectCommand = MakeAsyncCommand<IReadOnlySet<BarcodeResult>>(async x =>
        {
            if ((x.Count > 0) && !Controller.PauseScanning)
            {
                Controller.PauseScanning = true;

                var barcode = x.First().DisplayValue;
                try
                {
                    var parser = new SettingParser(barcode);
                    if (parser.TryGetString(nameof(ApiEndPoint), out var apiEndPoint))
                    {
                        settings.ApiEndPoint = apiEndPoint;
                        apiContext.BaseAddress = new Uri(apiEndPoint);
                        ApiEndPoint = apiEndPoint;
                    }
                    if (parser.TryGetString(nameof(GrpcEndPoint), out var grpcEndPoint))
                    {
                        settings.GrpcEndPoint = grpcEndPoint;
                        GrpcEndPoint = grpcEndPoint;
                    }
                    if (parser.TryGetString(nameof(OtelEndPoint), out var otelEndPoint))
                    {
                        settings.OtelEndPoint = otelEndPoint;
                        OtelEndPoint = otelEndPoint;
                        telemetryControl.EndPoint = settings.GetTelemetryEndPoint();
                    }
                    if (parser.TryGetString(nameof(AIServiceEndPoint), out var aiServiceEndPoint))
                    {
                        settings.AIServiceEndPoint = aiServiceEndPoint;
                        AIServiceEndPoint = aiServiceEndPoint;
                    }
                    if (parser.TryGetString(nameof(AIServiceKey), out var aiServiceKey))
                    {
                        await settings.SetAIServiceKeyAsync(aiServiceKey);
                        AIServiceKey = aiServiceKey;
                    }
                    if (parser.TryGetString(nameof(OllamaEndPoint), out var ollamaEndPoint))
                    {
                        settings.OllamaEndPoint = ollamaEndPoint;
                        OllamaEndPoint = ollamaEndPoint;
                    }
                    if (parser.TryGetString(nameof(OllamaModel), out var ollamaModel))
                    {
                        settings.OllamaModel = ollamaModel;
                        OllamaModel = ollamaModel;
                    }

                    if (parser.TryGetString(nameof(SshHost), out var sshHost))
                    {
                        settings.SshHost = sshHost;
                    }
                    if (parser.TryGetInt(nameof(Settings.SshPort), out var sshPort))
                    {
                        settings.SshPort = sshPort;
                    }
                    if (parser.TryGetString(nameof(SshUser), out var sshUser))
                    {
                        settings.SshUser = sshUser;
                        SshUser = sshUser;
                    }
                    if (parser.TryGetString(nameof(SshPassword), out var sshPassword))
                    {
                        await settings.SetSshPasswordAsync(sshPassword);
                        SshPassword = sshPassword;
                    }

                    SshHost = FormatSshHost(settings);
                }
                catch (UriFormatException)
                {
                    // Do nothing
                }

                await Task.Delay(DetectInterval);
                Controller.PauseScanning = false;
            }
        });
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            ApiEndPoint = settings.ApiEndPoint;
            GrpcEndPoint = settings.GrpcEndPoint;
            OtelEndPoint = settings.OtelEndPoint;
            TelemetryEnabled = settings.TelemetryEnabled;
            AIServiceEndPoint = settings.AIServiceEndPoint;
            AIServiceKey = await settings.GetAIServiceKeyAsync() ?? string.Empty;
            OllamaEndPoint = settings.OllamaEndPoint;
            OllamaModel = settings.OllamaModel;
            SshHost = FormatSshHost(settings);
            SshUser = settings.SshUser;
            SshPassword = await settings.GetSshPasswordAsync() ?? string.Empty;
        }
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (await Permissions.RequestCameraAsync())
        {
            Controller.Enable = true;
        }
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        Controller.Enable = false;
        Controller.PauseScanning = false;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static string FormatSshHost(Settings settings) =>
        String.IsNullOrEmpty(settings.SshHost) ? string.Empty : $"{settings.SshHost}:{settings.SshPort}";
}
