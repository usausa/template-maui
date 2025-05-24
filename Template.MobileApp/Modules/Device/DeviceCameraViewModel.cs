namespace Template.MobileApp.Modules.Device;

using Camera.MAUI;

using Template.MobileApp.Components.Storage;

public sealed class DeviceCameraViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IStorageManager storageManager;

    public CameraController Camera { get; } = new();

    public IObserveCommand TorchCommand { get; }
    public IObserveCommand MirrorCommand { get; }
    public IObserveCommand FlashModeCommand { get; }
    public IObserveCommand ZoomCommand { get; }

    public DeviceCameraViewModel(
        IDialog dialog,
        IStorageManager storageManager)
    {
        this.dialog = dialog;
        this.storageManager = storageManager;

        TorchCommand = MakeDelegateCommand(() => Camera.Torch = !Camera.Torch);
        MirrorCommand = MakeDelegateCommand(() => Camera.Mirror = !Camera.Mirror);
        FlashModeCommand = MakeDelegateCommand(SwitchFlashMode);
        ZoomCommand = MakeDelegateCommand(SwitchZoom, () => Camera.Camera is not null);
        Observe(Camera.AsObservable(), ZoomCommand);
    }

    // ReSharper disable once AsyncVoidMethod
    public override async void OnNavigatedTo(INavigationContext context)
    {
        await Navigator.PostActionAsync(() => BusyState.UsingAsync(() => Camera.StartPreviewAsync()));
    }

    // ReSharper disable once AsyncVoidMethod
    public override async void OnNavigatingFrom(INavigationContext context)
    {
        await Camera.StopPreviewAsync();
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
        await Camera.StopPreviewAsync();
        await Camera.SwitchPositionAsync();
        await Camera.StartPreviewAsync();
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
