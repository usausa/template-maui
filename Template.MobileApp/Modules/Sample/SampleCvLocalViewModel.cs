namespace Template.MobileApp.Modules.Sample;

using Template.MobileApp.Graphics;
using Template.MobileApp.Helpers;
using Template.MobileApp.Usecase;

public sealed partial class SampleCvLocalViewModel : AppViewModelBase
{
    private readonly CognitiveUsecase cognitiveUsecase;

    [ObservableProperty]
    public partial bool IsPreview { get; set; } = true;

    public SKBitmapImageSource Image { get; } = new();

    public CameraController Controller { get; } = new();

    public DetectGraphics Graphics { get; } = new();

    public SampleCvLocalViewModel(
        CognitiveUsecase cognitiveUsecase)
    {
        this.cognitiveUsecase = cognitiveUsecase;
        Disposables.Add(Controller.AsObservable(nameof(Controller.Selected)).Subscribe(_ =>
        {
            // Select minimum resolution
            var resolutions = Controller.Selected?.SupportedResolutions ?? [];
            var size = Size.Zero;
            foreach (var resolution in resolutions)
            {
                if ((resolution.Width < size.Width) || (resolution.Height < size.Height) || size.IsZero)
                {
                    size = resolution;
                }
            }

            Controller.CaptureResolution = size;
        }));
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (IsPreview)
        {
            await Controller.StartPreviewAsync().ConfigureAwait(true);
        }
    }

    public override async Task OnNavigatingFromAsync(INavigationContext context)
    {
        if (IsPreview)
        {
            await Controller.StopPreviewAsync().ConfigureAwait(true);
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

    protected override Task OnNotifyFunction4()
    {
        return BusyState.Using(async () =>
        {
            if (IsPreview)
            {
                // Capture
                await using var input = await Controller.CaptureAsync().ConfigureAwait(true);
                if (input is null)
                {
                    return;
                }

                await Controller.StopPreviewAsync().ConfigureAwait(true);

                // Bitmap
                using var bitmap = ImageHelper.ToNormalizeBitmap(input);
                Image.Bitmap = bitmap;

                // Detect
                var results = await cognitiveUsecase.DetectAsync(bitmap).ConfigureAwait(true);

                // Update
                Graphics.Update(bitmap.Width, bitmap.Height, results.Where(static x => x.Score >= 0.5).ToArray());

                IsPreview = false;
            }
            else
            {
                await Controller.StartPreviewAsync().ConfigureAwait(true);
                IsPreview = true;
            }
        });
    }
}
