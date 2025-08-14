namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics;
using Template.MobileApp.Helpers;
using Template.MobileApp.Usecase;

public sealed partial class UITreeMapViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly SampleUsecase sampleUsecase;

    [ObservableProperty]
    public partial bool IsPreview { get; set; } = true;

    public CameraController Controller { get; } = new();

    public ColorTreeMapGraphics Graphics { get; } = new();

    public SKBitmapImageSource Image { get; } = new();

    public UITreeMapViewModel(
        IDialog dialog,
        SampleUsecase sampleUsecase)
    {
        this.dialog = dialog;
        this.sampleUsecase = sampleUsecase;

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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu);

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

            using var loading = dialog.Indicator();

            // ReSharper disable once AccessToDisposedClosure
            var (bitmap, colors) = await Task.Run(() =>
            {
                var bitmap = ImageHelper.ToNormalizeBitmap(input);

                using var resized = ImageHelper.Resize(bitmap, 0.25);
                var colors = sampleUsecase.ClusterColors(resized, 20, 5, 1e-3f);

                return (bitmap, colors);
            }).ConfigureAwait(true);

            // Update
            Image.Bitmap = bitmap;
            Graphics.Update(TreeMapNode<ColorCount>.Build(colors, static x => x.Count));
        }
        else
        {
            Controller.StartPreview();
        }

        IsPreview = !IsPreview;
    }
}
