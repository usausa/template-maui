namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;

public sealed class DeviceOcrViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IOcrReader ocrReader;

    public CameraController Controller { get; } = new();

    public DeviceOcrViewModel(
        IDialog dialog,
        IOcrReader ocrReader)
    {
        this.dialog = dialog;
        this.ocrReader = ocrReader;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction4()
    {
        await using var input = await Controller.CaptureAsync().ConfigureAwait(true);
        if (input is null)
        {
            return;
        }

        var text = await ocrReader.ReadTextAsync(input);
        if (!String.IsNullOrEmpty(text))
        {
            await dialog.InformationAsync(text);
        }
    }
}
