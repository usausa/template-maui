namespace Template.MobileApp.Modules.UI;

using System.Diagnostics;

using SkiaSharp;

using Template.MobileApp.Graphics;
using Template.MobileApp.Helpers;
using Template.MobileApp.Usecase;

public sealed partial class UITreeMapViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly SampleUsecase sampleUsecase;

    [ObservableProperty]
    public partial bool IsPreview { get; set; } = true;

    [ObservableProperty]
    public partial ImageSource? Image { get; set; }

    public CameraController Controller { get; } = new();

    public ColorTreeMapGraphics Graphics { get; } = new();

    public UITreeMapViewModel(
        IDialog dialog,
        SampleUsecase sampleUsecase)
    {
        this.dialog = dialog;
        this.sampleUsecase = sampleUsecase;

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

                using var loading = dialog.Indicator();

                // ReSharper disable once AccessToDisposedClosure
                var (data, colors) = await Task.Run(() =>
                {
                    using var bitmap = ImageHelper.ToNormalizeBitmap(input);
                    using var resized = ImageHelper.Resize(bitmap, 0.25);

                    var watch = Stopwatch.StartNew();
                    var colors = sampleUsecase.ClusterColors(resized, 20, 5, 1e-3f);
                    Debug.WriteLine($"Color clustering completed. elapsed=[{watch.ElapsedMilliseconds}]");

                    var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 100);

                    return (data, colors);
                }).ConfigureAwait(true);

                // Update
                Image = ImageSource.FromStream(() => data.AsStream());
                Graphics.Update(TreeMapNode<ColorCount>.Build(colors, static x => x.Count));

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
