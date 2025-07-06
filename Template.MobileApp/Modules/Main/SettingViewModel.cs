namespace Template.MobileApp.Modules.Main;

using BarcodeScanning;

using Template.MobileApp.Helpers;
using Template.MobileApp.Services;

public sealed partial class SettingViewModel : AppViewModelBase
{
    public BarcodeController Controller { get; } = new();

    [ObservableProperty]
    public partial string ApiEndPoint { get; set; }

    [ObservableProperty]
    public partial string AIServiceEndPoint { get; set; }

    [ObservableProperty]
    public partial string AIServiceKey { get; set; }

    public IObserveCommand DetectCommand { get; }

    public SettingViewModel(
        ApiContext apiContext,
        Settings settings)
    {
        Controller.AimMode = true;
        Controller.VibrationOnDetect = true;
        Controller.CaptureNextFrame = false;

        ApiEndPoint = settings.ApiEndPoint;
        AIServiceEndPoint = settings.AIServiceEndPoint;
        AIServiceKey = settings.AIServiceKey;

        DetectCommand = MakeDelegateCommand<IReadOnlySet<BarcodeResult>>(x =>
        {
            if (x.Count > 0)
            {
                var barcode = x.First().DisplayValue;
                try
                {
                    var parser = new SettingParser(barcode);
                    if (parser.TryGetString(nameof(ApiEndPoint), out var apiEndPoint))
                    {
                        settings.ApiEndPoint = apiEndPoint;
                        apiContext.BaseAddress = new Uri(apiEndPoint);
                    }
                    if (parser.TryGetString(nameof(AIServiceEndPoint), out var aiServiceEndPoint))
                    {
                        settings.AIServiceEndPoint = aiServiceEndPoint;
                    }
                    if (parser.TryGetString(nameof(AIServiceKey), out var aiServiceKey))
                    {
                        settings.AIServiceKey = aiServiceKey;
                    }
                }
                catch (UriFormatException)
                {
                    // Do nothing
                }
            }
        });
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        Controller.Enable = true;
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        Controller.Enable = false;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
