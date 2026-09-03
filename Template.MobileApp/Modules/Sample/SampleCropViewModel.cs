namespace Template.MobileApp.Modules.Sample;

using Microsoft.Maui.Graphics.Platform;

using Template.MobileApp.Graphics.Drawing;

public sealed partial class SampleCropViewModel : AppViewModelBase
{
    private bool loaded;

    public CropDrawing Crop { get; } = new();

    [ObservableProperty]
    public partial ImageSource? CroppedImage { get; private set; }

    [ObservableProperty]
    public partial string ResultText { get; private set; } = "枠を調整して書き出しできます";

    public IObserveCommand ExportCommand { get; }
    public IObserveCommand ResetCommand { get; }

    public SampleCropViewModel()
    {
        Disposables.Add(Crop);

        ExportCommand = MakeDelegateCommand(Export);
        ResetCommand = MakeDelegateCommand(Crop.Reset);
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!loaded)
        {
            loaded = true;
            await using var stream = await FileSystem.OpenAppPackageFileAsync(Path.Combine("Avatar", "mofusand.jpg"));
            Crop.SetImage(PlatformImage.FromStream(stream));
        }
    }

    private void Export()
    {
        using var buffer = new MemoryStream();
        var (width, height) = Crop.ExportCrop(buffer);
        if (width == 0)
        {
            return;
        }

        var bytes = buffer.ToArray();
        CroppedImage = ImageSource.FromStream(() => new MemoryStream(bytes));
        ResultText = $"{width} x {height} px / {bytes.Length:N0} bytes";
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
