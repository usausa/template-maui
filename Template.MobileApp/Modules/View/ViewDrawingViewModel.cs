namespace Template.MobileApp.Modules.View;

public sealed class ViewDrawingViewModel : AppViewModelBase
{
    public DrawingController Controller { get; } = new();

    public SKBitmapImageSource Image { get; } = new();

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
                Image.Bitmap = SKBitmap.Decode(stream);
            }
        }
        else
        {
            Image.Bitmap = null;
        }
    }
}
