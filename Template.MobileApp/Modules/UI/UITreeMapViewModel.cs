namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;
using Template.MobileApp.Helpers;

public sealed partial class UITreeMapViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    [ObservableProperty]
    public partial bool IsPreview { get; set; } = true;

    [ObservableProperty]
    public partial int CaptureCount { get; set; }

    public CameraController Controller { get; } = new();

    public ColorTreeMapDrawing Drawing { get; } = new();

    public SKBitmapImageSource Image { get; } = new();

    public UITreeMapViewModel(IDialog dialog)
    {
        this.dialog = dialog;

        Disposables.Add(Drawing);
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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

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

            using var loading = dialog.Indicator();

            // ReSharper disable once AccessToDisposedClosure
            var (bitmap, colors) = await Task.Run(() =>
            {
                var bitmap = ImageHelper.ToNormalizeBitmap(input);

                using var extractor = new ColorExtractor();
                var colors = extractor.Extract(bitmap, 20);

                return (bitmap, colors);
            }).ConfigureAwait(true);

            // Update
            ImageHelper.ReplaceBitmap(Image, bitmap);
            Drawing.Update(TreeMapNode<ColorCount>.Build(colors, static x => x.Count));

            // シャッターフラッシュのトリガー
            CaptureCount++;
        }
        else
        {
            await Controller.StartPreviewAsync();
        }

        IsPreview = !IsPreview;
    }
}
