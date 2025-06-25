namespace Template.MobileApp.Modules.Sample;

using SkiaSharp;

using Template.MobileApp.Helpers;
using Template.MobileApp.Usecase;

// TODO default quality ?

public sealed partial class SampleCvLocalViewModel : AppViewModelBase
{
    private readonly CognitiveUsecase cognitiveUsecase;

    [ObservableProperty]
    public partial bool IsPreview { get; set; } = true;

    [ObservableProperty]
    public partial ImageSource? Image { get; set; }

    public CameraController Controller { get; } = new();

    public SampleCvLocalViewModel(
        CognitiveUsecase cognitiveUsecase)
    {
        this.cognitiveUsecase = cognitiveUsecase;
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

    protected override async Task OnNotifyFunction4()
    {
        if (IsPreview)
        {
            await using var input = await Controller.CaptureAsync().ConfigureAwait(true);
            if (input is null)
            {
                return;
            }

            await Controller.StopPreviewAsync().ConfigureAwait(true);

            await BusyState.Using(async () =>
            {
                using var bitmap = ImageHelper.ToNormalizeBitmap(input);
                var results = await cognitiveUsecase.DetectAsync(bitmap).ConfigureAwait(true);

                // TODO
                using var canvas = new SKCanvas(bitmap);
                using var paint = new SKPaint();
                paint.Color = SKColors.Red;
                paint.StrokeWidth = 25;
                paint.IsStroke = true;
                foreach (var result in results)
                {
                    System.Diagnostics.Debug.WriteLine($"{result.Score} : {result.Left} {result.Top} {result.Right} {result.Bottom}");
                    if (result.Score >= 0.5f)
                    {
                        canvas.DrawRect(new SKRect(bitmap.Width * result.Left, bitmap.Height * result.Top, bitmap.Width * result.Right, bitmap.Height * result.Bottom), paint);
                    }
                }

                var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 100);

                Image = ImageSource.FromStream(() => data.AsStream());
            });

            IsPreview = false;
        }
        else
        {
            await Controller.StartPreviewAsync().ConfigureAwait(true);
            IsPreview = true;
        }
    }
}
