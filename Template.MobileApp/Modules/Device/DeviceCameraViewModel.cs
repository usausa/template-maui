namespace Template.MobileApp.Modules.Device;

using Camera.MAUI;

using Template.MobileApp.Components.Storage;

public class DeviceCameraViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IStorageManager storageManager;

    public CameraController Camera { get; } = new();

    public ICommand TorchCommand { get; }
    public ICommand MirrorCommand { get; }
    public ICommand FlashModeCommand { get; }
    public ICommand ZoomCommand { get; }

    public DeviceCameraViewModel(
        ApplicationState applicationState,
        IDialog dialog,
        IStorageManager storageManager)
        : base(applicationState)
    {
        this.dialog = dialog;
        this.storageManager = storageManager;

        Camera.Preview = true;

        TorchCommand = MakeDelegateCommand(() => Camera.Torch = !Camera.Torch);
        MirrorCommand = MakeDelegateCommand(() => Camera.Mirror = !Camera.Mirror);
        FlashModeCommand = MakeDelegateCommand(SwitchFlashMode);
        ZoomCommand = MakeDelegateCommand(SwitchZoom, () => Camera.Camera is not null).Observe(Camera);
    }

    public override void OnNavigatedTo(INavigationContext context)
    {
        Camera.Preview = true;
    }

    public override void OnNavigatingFrom(INavigationContext context)
    {
        Camera.Preview = false;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction2()
    {
        var file = Path.Combine(storageManager.PublicFolder, "shot.jpg");
        var result = await Camera.SaveSnapshotAsync(file);
        if (result)
        {
            var fi = new FileInfo(file);
            await dialog.InformationAsync($"Save image success. size={fi.Length}");
        }
        else
        {
            await dialog.InformationAsync("Save image failed.");
        }
    }

    protected override Task OnNotifyFunction3()
    {
        Camera.FocusRequest();
        return Task.CompletedTask;
    }

    protected override async Task OnNotifyFunction4()
    {
        await Camera.SwitchPositionAsync();
        Camera.Zoom = 1;
    }

    private void SwitchFlashMode()
    {
        Camera.FlashMode = Camera.FlashMode switch
        {
            FlashMode.Auto => FlashMode.Enabled,
            FlashMode.Enabled => FlashMode.Disabled,
            FlashMode.Disabled => FlashMode.Auto,
            _ => Camera.FlashMode
        };
    }

    private void SwitchZoom()
    {
        Camera.Zoom = Camera.Zoom < Math.Min(Camera.Camera?.MaxZoomFactor ?? 1, 5) ? Camera.Zoom + 1 : 1;
    }
}
