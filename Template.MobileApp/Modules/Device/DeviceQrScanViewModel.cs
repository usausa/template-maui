namespace Template.MobileApp.Modules.Device;

using BarcodeScanning;

using Plugin.Maui.Audio;

public sealed partial class DeviceQrScanViewModel : AppViewModelBase
{
    private readonly IFileSystem fileSystem;

    private readonly IAudioManager audioManager;

#pragma warning disable CA2213
    private IAudioPlayer? audioPlayer;
#pragma warning restore CA2213

    public BarcodeController Controller { get; } = new();

    [ObservableProperty]
    public partial string Barcode { get; set; } = string.Empty;

    public IObserveCommand DetectCommand { get; }

    public DeviceQrScanViewModel(
        IFileSystem fileSystem,
        IAudioManager audioManager)
    {
        this.fileSystem = fileSystem;
        this.audioManager = audioManager;

        Controller.AimMode = true;

        DetectCommand = MakeDelegateCommand<IReadOnlySet<BarcodeResult>>(x =>
        {
            if (x.Count > 0)
            {
                var barcode = x.First().DisplayValue;
                if (Barcode != barcode)
                {
                    Barcode = barcode;
                    audioPlayer?.Play();
                }
            }
        });
    }

    // ReSharper disable once AsyncVoidMethod
    public override async void OnNavigatedTo(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            audioPlayer = audioManager.CreatePlayer(await fileSystem.OpenAppPackageFileAsync("Read.wav"));
            Disposables.Add(audioPlayer);
        }

        Controller.Enable = true;
    }

    public override void OnNavigatingFrom(INavigationContext context)
    {
        Controller.Enable = false;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
