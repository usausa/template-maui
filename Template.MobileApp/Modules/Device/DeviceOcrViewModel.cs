namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;

public sealed partial class DeviceOcrViewModel : AppViewModelBase
{
    // Field

    private readonly IDialog dialog;

    private readonly IOcrReader ocrReader;

    // Property

    public CameraController Controller { get; } = new();

    [ObservableProperty]
    public partial string RecognizedText { get; set; }

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial bool IsCameraEnabled { get; set; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DeviceOcrViewModel(
        IDialog dialog,
        IOcrReader ocrReader)
    {
        this.dialog = dialog;
        this.ocrReader = ocrReader;

        RecognizedText = string.Empty;
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
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
            await using var input = await Controller.CaptureWithTimeoutAsync().ConfigureAwait(true);
            if (input is null)
            {
                await dialog.InformationAsync("Capture failed. Try again.");
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
