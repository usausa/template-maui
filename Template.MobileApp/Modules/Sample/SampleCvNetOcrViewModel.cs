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
    //public DetectGraphics Graphics { get; } = new();

    public SampleCvNetOcrViewModel()
    {
        Disposables.Add(Controller.AsObservable(nameof(Controller.Selected)).Subscribe(_ => Controller.SelectMinimumResolution()));
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (IsPreview)
        {
            Controller.StartPreview();
        }
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        if (IsPreview)
        {
            Controller.StopPreview();
        }
        return Task.CompletedTask;
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

            Controller.StopPreview();

            // Bitmap
            using var bitmap = ImageHelper.ToNormalizeBitmap(input);
            Image.Bitmap = bitmap;

            // TODO

            IsPreview = false;
        }
        else
        {
            Controller.StartPreview();
            IsPreview = true;
        }
    }
}
