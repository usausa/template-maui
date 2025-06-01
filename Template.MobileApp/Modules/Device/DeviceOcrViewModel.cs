namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components.Ocr;

public sealed class DeviceOcrViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IOcrManager ocrManager;

    public CameraController Controller { get; } = new();

    public DeviceOcrViewModel(
        IDialog dialog,
        IOcrManager ocrManager)
    {
        this.dialog = dialog;
        this.ocrManager = ocrManager;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction4()
    {
        await using var input = await Controller.CaptureAsync();
        if (input is null)
        {
            return;
        }

        var text = await ocrManager.ReadTextAsync(input);
        if (!String.IsNullOrEmpty(text))
        {
            await dialog.InformationAsync(text);
        }
    }
}
