namespace Template.MobileApp.Modules.Device;

public sealed partial class DeviceCameraViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    public CameraController Controller { get; } = new();

    [ObservableProperty]
    public partial bool IsPreview { get; set; }

    public IObserveCommand TorchCommand { get; }
    public IObserveCommand FlashModeCommand { get; }
    public IObserveCommand ZoomOutCommand { get; }
    public IObserveCommand ZoomInCommand { get; }

    public DeviceCameraViewModel(
        IDialog dialog)
    {
        this.dialog = dialog;

        TorchCommand = MakeDelegateCommand(Controller.ToggleTorch);
        FlashModeCommand = MakeDelegateCommand(Controller.SwitchFlashMode);
        ZoomOutCommand = MakeDelegateCommand(Controller.ZoomOut);
        ZoomInCommand = MakeDelegateCommand(Controller.ZoomIn);
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        Controller.StartPreview();
        IsPreview = true;
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        Controller.StopPreview();
        IsPreview = false;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction2()
    {
        await Controller.SwitchCameraAsync().ConfigureAwait(true);
    }

    protected override Task OnNotifyFunction3()
    {
        if (IsPreview)
        {
            Controller.StopPreview();
        }
        else
        {
            Controller.StartPreview();
        }

        IsPreview = !IsPreview;

        return Task.CompletedTask;
    }

    protected override async Task OnNotifyFunction4()
    {
        await using var input = await Controller.CaptureAsync().ConfigureAwait(true);
        if (input is not null)
        {
            await dialog.InformationAsync($"Save image success. size={input.Length}");
        }
    }
}
