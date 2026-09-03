namespace Template.MobileApp.Modules.Sample;

using Template.MobileApp.Graphics.Drawing;
using Template.MobileApp.Helpers;
using Template.MobileApp.Usecase;

public sealed partial class SampleCvLocalViewModel : AppViewModelBase
{
    private readonly CognitiveUsecase cognitiveUsecase;

    [ObservableProperty]
    public partial bool IsPreview { get; set; } = true;

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    public CameraController Controller { get; } = new();

    public DetectDrawing Drawing { get; } = new();

    public SKBitmapImageSource Image { get; } = new();

    public SampleCvLocalViewModel(
        CognitiveUsecase cognitiveUsecase)
    {
        this.cognitiveUsecase = cognitiveUsecase;
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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

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
        // 推論中の再入でReplaceBitmapが使用中のビットマップを破棄しないようガードする
        if (IsProcessing)
        {
            return;
        }

        IsProcessing = true;
        try
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

                // Bitmap
                var bitmap = ImageHelper.ToNormalizeBitmap(input);
                ImageHelper.ReplaceBitmap(Image, bitmap);

                // Detect
                var results = await cognitiveUsecase.DetectAsync(bitmap).ConfigureAwait(true);

                // Update
                Drawing.Update(bitmap.Width, bitmap.Height, results.Where(static x => x.Score >= 0.5).ToArray());
            }
            else
            {
                await Controller.StartPreviewAsync();
            }

            IsPreview = !IsPreview;
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
