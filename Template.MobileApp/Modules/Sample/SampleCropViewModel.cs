namespace Template.MobileApp.Modules.Sample;

using Microsoft.Maui.Graphics.Platform;

using Template.MobileApp.Graphics.Drawing;

public sealed partial class SampleCropViewModel : AppViewModelBase
{
    public CropDrawing Crop { get; } = new();

    [ObservableProperty]
    public partial ImageSource? CroppedImage { get; private set; }

    [ObservableProperty]
    public partial string ResultText { get; private set; } = "枠を調整して書き出しできます";

    public IObserveCommand ExportCommand { get; }
    public IObserveCommand ResetCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SampleCropViewModel()
    {
        Disposables.Add(Crop);

        ExportCommand = MakeDelegateCommand(Export);
        ResetCommand = MakeDelegateCommand(Crop.Reset);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(Path.Combine("Avatar", "mofusand.jpg"));
            Crop.SetImage(PlatformImage.FromStream(stream));
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Export()
    {
        using var buffer = new MemoryStream();
        if (!Crop.ExportCrop(buffer).TryGetValue(out var size))
        {
            return;
        }

        var bytes = buffer.ToArray();
        CroppedImage = ImageSource.FromStream(() => new MemoryStream(bytes));
        ResultText = $"{size.Width} x {size.Height} px / {bytes.Length:N0} bytes";
    }
}
