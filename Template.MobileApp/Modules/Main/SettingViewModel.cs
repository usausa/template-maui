namespace Template.MobileApp.Modules.Main;

using BarcodeScanning;

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
    public partial string? AIServiceEndPoint { get; set; }

    [ObservableProperty]
    public partial string? AIServiceKey { get; set; }

    [ObservableProperty]
    public partial string? OllamaEndPoint { get; set; }

    [ObservableProperty]
    public partial string? OllamaModel { get; set; }

    [ObservableProperty]
    public partial string? ScpHost { get; set; }

    [ObservableProperty]
    public partial string? ScpUser { get; set; }

    [ObservableProperty]
    public partial string? ScpPassword { get; set; }

    public IObserveCommand DetectCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SettingViewModel(
        ApiContext apiContext,
        Settings settings)
    {
        this.settings = settings;

        Controller.AimMode = true;
        Controller.VibrationOnDetect = true;
        Controller.CaptureNextFrame = false;

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

                    if (parser.TryGetString(nameof(ScpHost), out var scpHost))
                    {
                        settings.ScpHost = scpHost;
                    }
                    if (parser.TryGetInt(nameof(Settings.ScpPort), out var scpPort))
                    {
                        settings.ScpPort = scpPort;
                    }
                    if (parser.TryGetString(nameof(ScpUser), out var scpUser))
                    {
                        settings.ScpUser = scpUser;
                        ScpUser = scpUser;
                    }
                    if (parser.TryGetString(nameof(ScpPassword), out var scpPassword))
                    {
                        await settings.SetScpPasswordAsync(scpPassword);
                        ScpPassword = scpPassword;
                    }

                    ScpHost = FormatScpHost(settings);
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
        ApiEndPoint = settings.ApiEndPoint;
        GrpcEndPoint = settings.GrpcEndPoint;
        OtelEndPoint = settings.OtelEndPoint;
        AIServiceEndPoint = settings.AIServiceEndPoint;
        AIServiceKey = await settings.GetAIServiceKeyAsync() ?? string.Empty;
        OllamaEndPoint = settings.OllamaEndPoint;
        OllamaModel = settings.OllamaModel;
        ScpHost = FormatScpHost(settings);
        ScpUser = settings.ScpUser;
        ScpPassword = await settings.GetScpPasswordAsync() ?? string.Empty;
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

    private static string FormatScpHost(Settings settings) =>
        String.IsNullOrEmpty(settings.ScpHost) ? string.Empty : $"{settings.ScpHost}:{settings.ScpPort}";
}
