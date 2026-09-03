namespace Template.MobileApp.Modules.View;

using Template.MobileApp.Helpers;

public sealed partial class ViewDrawingViewModel : AppViewModelBase
{
    public DrawingController Controller { get; } = new();

    public SKBitmapImageSource Image { get; } = new();

    [ObservableProperty]
    public partial Color LineColor { get; set; } = Colors.Black;

    [ObservableProperty]
    public partial double LineWidth { get; set; } = 5d;

    [ObservableProperty]
    public partial bool HasImage { get; set; }

    public IObserveCommand SelectColorCommand { get; }

    public ViewDrawingViewModel()
    {
        SelectColorCommand = MakeDelegateCommand<Color>(x => LineColor = x);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ImageHelper.ReplaceBitmap(Image, null);
        }

        base.Dispose(disposing);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction3()
    {
        Controller.Lines.Clear();
        return Task.CompletedTask;
    }

    protected override async Task OnNotifyFunction4()
    {
        if (Controller.Lines.Count > 0)
        {
            var stream = await Controller.GetImageStream();
            if (stream is not null)
            {
                ImageHelper.ReplaceBitmap(Image, SKBitmap.Decode(stream));
                HasImage = true;
            }
        }
        else
        {
            ImageHelper.ReplaceBitmap(Image, null);
            HasImage = false;
        }
    }
}
