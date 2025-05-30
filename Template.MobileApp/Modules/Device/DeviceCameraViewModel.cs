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

    // ReSharper disable once AsyncVoidMethod
    public override async void OnNavigatedTo(INavigationContext context)
    {
        await Navigator.PostActionAsync(() => BusyState.UsingAsync(async () =>
        {
            await Controller.StartPreviewAsync();
            IsPreview = true;
        }));
    }

    // ReSharper disable once AsyncVoidMethod
    public override async void OnNavigatingFrom(INavigationContext context)
    {
        await Controller.StopPreviewAsync();
        IsPreview = false;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction2()
    {
        await Controller.SwitchCameraAsync();
    }

    protected override async Task OnNotifyFunction3()
    {
        if (IsPreview)
        {
            await Controller.StopPreviewAsync();
            IsPreview = false;
        }
        else
        {
            await Controller.StartPreviewAsync();
            IsPreview = true;
        }
    }

    protected override async Task OnNotifyFunction4()
    {
        await using var input = await Controller.CaptureAsync();
        if (input is not null)
        {
            await dialog.InformationAsync($"Save image success. size={input.Length}");
        }
    }
}
