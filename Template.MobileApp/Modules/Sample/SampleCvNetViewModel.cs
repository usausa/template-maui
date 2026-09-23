namespace Template.MobileApp.Modules.Sample;

using Template.MobileApp.Graphics.Drawing;
using Template.MobileApp.Helpers;
using Template.MobileApp.Usecase;

public sealed partial class SampleCvNetViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly AzureVisionUsecase visionUsecase;

    [ObservableProperty]
    public partial CaptureState State { get; set; }

    [ObservableProperty]
    public partial VisionFeature Feature { get; set; }

    [ObservableProperty]
    public partial string TagsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int CaptureCount { get; set; }

    public SKBitmapImageSource Image { get; } = new();

    public CameraController Controller { get; } = new();

    public DetectDrawing Drawing { get; } = new();

    public IObserveCommand SelectFeatureCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SampleCvNetViewModel(
        IDialog dialog,
        AzureVisionUsecase visionUsecase)
    {
        this.dialog = dialog;
        this.visionUsecase = visionUsecase;

        SelectFeatureCommand = MakeAsyncCommand<VisionFeature>(SelectFeatureAsync);

        Disposables.Add(Controller.AsObservable(nameof(Controller.Selected)).Subscribe(_ => Controller.SelectMinimumResolution()));
        Disposables.Add(new DelegateDisposable(() => ImageHelper.ReplaceBitmap(Image, null)));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if ((State == CaptureState.Preview) && await Permissions.RequestCameraAsync())
        {
            await Controller.StartPreviewAsync();
        }
    }

    public override async Task OnNavigatingFromAsync(INavigationContext context)
    {
        if (State is CaptureState.Preview or CaptureState.Capturing)
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

    protected override Task OnNotifyFunction4()
    {
        return State switch
        {
            CaptureState.Preview => CaptureAsync(),
            CaptureState.Result => RestartPreviewAsync(),
            _ => Task.CompletedTask
        };
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private Task SelectFeatureAsync(VisionFeature feature)
    {
        if ((Feature == feature) || (State is CaptureState.Capturing or CaptureState.Analyzing))
        {
            return Task.CompletedTask;
        }

        Feature = feature;
        return State == CaptureState.Result ? RestartPreviewAsync() : Task.CompletedTask;
    }

    private async Task CaptureAsync()
    {
        State = CaptureState.Capturing;
        try
        {
            await using var input = await Controller.CaptureWithTimeoutAsync().ConfigureAwait(true);
            if (input is null)
            {
                State = CaptureState.Preview;
                await dialog.InformationAsync("撮影できませんでした。もう一度お試しください。");
                return;
            }

            await Controller.StopPreviewAsync();

            var bitmap = ImageHelper.ToNormalizeBitmap(input);
            ImageHelper.ReplaceBitmap(Image, bitmap);
            State = CaptureState.Analyzing;

            CaptureCount++;

            await DetectAsync(bitmap);
            State = CaptureState.Result;
        }
        finally
        {
            State = State switch
            {
                CaptureState.Capturing => CaptureState.Preview,
                CaptureState.Analyzing => CaptureState.Result,
                _ => State
            };
        }
    }

    private async Task RestartPreviewAsync()
    {
        State = CaptureState.Capturing;
        TagsText = string.Empty;
        await Controller.StartPreviewAsync();
        State = CaptureState.Preview;
    }

    private async Task DetectAsync(SKBitmap bitmap)
    {
        Drawing.Update(bitmap.Width, bitmap.Height, []);
        TagsText = string.Empty;

        Error? error;
        using (BusyState.Begin())
        {
            error = await AnalyzeAsync(bitmap).ConfigureAwait(true);
        }

        if (error is not null)
        {
            await dialog.InformationAsync($"解析に失敗しました。\n{error.Message}");
        }
    }

    private async Task<Error?> AnalyzeAsync(SKBitmap bitmap)
    {
        if (Feature == VisionFeature.Tags)
        {
            var tags = await visionUsecase.DetectTagsAsync(bitmap).ConfigureAwait(true);
            if (!tags.IsSuccess)
            {
                return tags.Error;
            }

            TagsText = tags.Value.Length > 0
                ? String.Join("\n", tags.Value.Select(static x => $"🏷 {x.Name}  {x.Confidence:P0}"))
                : "タグは検出されませんでした";
            return null;
        }

        var results = await (Feature switch
        {
            VisionFeature.People => visionUsecase.DetectPeopleAsync(bitmap),
            VisionFeature.Ocr => visionUsecase.ReadTextAsync(bitmap),
            _ => visionUsecase.DetectObjectsAsync(bitmap)
        }).ConfigureAwait(true);
        if (!results.IsSuccess)
        {
            return results.Error;
        }

        Drawing.Update(bitmap.Width, bitmap.Height, results.Value);
        return null;
    }
}
