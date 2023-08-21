namespace Template.MobileApp.Modules.Device;

using Camera.MAUI;

using Plugin.Maui.Audio;

public class DeviceQrScanViewModel : AppViewModelBase
{
    private readonly IFileSystem fileSystem;

    private readonly IAudioManager audioManager;

#pragma warning disable CA2213
    private IAudioPlayer? audioPlayer;
#pragma warning restore CA2213

    public CameraController Camera { get; }

    public NotificationValue<string> Barcode { get; } = new();

    public DeviceQrScanViewModel(
        ApplicationState applicationState,
        IFileSystem fileSystem,
        IAudioManager audioManager)
        : base(applicationState)
    {
        this.fileSystem = fileSystem;
        this.audioManager = audioManager;

        Camera = new CameraController(CameraPosition.Back, MakeDelegateCommand<ZXing.Result>(x =>
        {
            Barcode.Value = x.Text;
            audioPlayer?.Play();
        }));
    }

    public override async void OnNavigatedTo(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            audioPlayer = audioManager.CreatePlayer(await fileSystem.OpenAppPackageFileAsync("Read.wav"));
            Disposables.Add(audioPlayer);
        }

        Camera.Preview = true;
        Camera.BarcodeDetection = true;
    }

    public override void OnNavigatingFrom(INavigationContext context)
    {
        Camera.Preview = false;
        Camera.BarcodeDetection = false;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
