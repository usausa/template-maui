namespace Template.MobileApp.Messaging;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

public interface IDrawingController : INotifyPropertyChanged
{
    Color LineColor { get; set; }

    float LineWidth { get; set; }

#pragma warning disable CA2227
    ObservableCollection<IDrawingLine> Lines { get; set; }
#pragma warning restore CA2227

    // Attach

    void Attach(DrawingView view);

    void Detach();
}

public sealed partial class DrawingController : ObservableObject, IDrawingController
{
    private DrawingView? drawing;

    // Property

    [ObservableProperty]
    public partial Color LineColor { get; set; } = Colors.Black;

    [ObservableProperty]
    public partial float LineWidth { get; set; } = 5;

    public ObservableCollection<IDrawingLine> Lines { get; set; } = new();

    // Attach

    void IDrawingController.Attach(DrawingView view)
    {
        drawing = view;
    }

    void IDrawingController.Detach()
    {
        drawing = null;
    }

    // Message

    public async ValueTask<Stream?> GetImageStream(CancellationToken cancel = default)
    {
        if (drawing is null)
        {
            return null;
        }

        return await drawing.GetImageStream(drawing.Width, drawing.Height, cancel);
    }
}
