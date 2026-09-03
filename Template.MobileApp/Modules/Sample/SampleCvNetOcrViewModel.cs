namespace Template.MobileApp.Modules.Sample;

using Template.MobileApp.Helpers;

public sealed partial class SampleCvNetOcrViewModel : AppViewModelBase
{
    // TODO
    //private readonly CognitiveUsecase cognitiveUsecase;

    [ObservableProperty]
    public partial bool IsPreview { get; set; } = true;

    public SKBitmapImageSource Image { get; } = new();

    public CameraController Controller { get; } = new();

    // TODO
    //public DetectDrawing Drawing { get; } = new();

    public SampleCvNetOcrViewModel()
    {
        Disposables.Add(Controller.AsObservable(nameof(Controller.Selected)).Subscribe(_ => Controller.SelectMinimumResolution()));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ImageHelper.ReplaceBitmap(Image, null);
        }

        base.Dispose(disposing);
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (IsPreview && await Permissions.RequestCameraAsync())
        {
            await Controller.StartPreviewAsync();
        }
    }

    public override async Task OnNavigatingFromAsync(INavigationContext context)
    {
        if (IsPreview)
        {
            await Controller.StopPreviewAsync();
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleCvNetMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Controller.ZoomOut();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        Controller.ZoomIn();
        return Task.CompletedTask;
    }

    protected override async Task OnNotifyFunction4()
    {
        if (IsPreview)
        {
            // Capture
            await using var input = await Controller.CaptureAsync().ConfigureAwait(true);
            if (input is null)
            {
                return;
            }

            await Controller.StopPreviewAsync();

            // Bitmap (所有権はImage側のため差し替え時に旧ビットマップを解放する)
            var bitmap = ImageHelper.ToNormalizeBitmap(input);
            ImageHelper.ReplaceBitmap(Image, bitmap);

            // TODO
        }
        else
        {
            await Controller.StartPreviewAsync();
        }

        IsPreview = !IsPreview;
    }
}
