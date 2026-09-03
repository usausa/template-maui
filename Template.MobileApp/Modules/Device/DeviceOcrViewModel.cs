namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;

public sealed partial class DeviceOcrViewModel : AppViewModelBase
{
    private readonly IOcrReader ocrReader;

    public CameraController Controller { get; } = new();

    [ObservableProperty]
    public partial string RecognizedText { get; set; }

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial bool IsCameraEnabled { get; set; }

    public DeviceOcrViewModel(
        IOcrReader ocrReader)
    {
        this.ocrReader = ocrReader;

        RecognizedText = string.Empty;
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        // CameraViewがプレビューを自動開始するため事前に権限を要求しておく
        IsCameraEnabled = await Permissions.RequestCameraAsync();
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction4()
    {
        if (IsProcessing || !IsCameraEnabled)
        {
            return;
        }

        IsProcessing = true;
        try
        {
            await using var input = await Controller.CaptureAsync().ConfigureAwait(true);
            if (input is null)
            {
                return;
            }

            var text = await ocrReader.ReadTextAsync(input);
            if (!String.IsNullOrEmpty(text))
            {
                RecognizedText = text;
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
