namespace Template.MobileApp.Messaging;

using CommunityToolkit.Maui.Core;

using Template.MobileApp.Helpers;

public sealed class GetImageStreamEventArgs : ValueTaskEventArgs<Stream?>
{
    public CancellationToken Token { get; set; } = CancellationToken.None;

    public Stream? Stream { get; set; }
}

public sealed partial class DrawingController : ObservableObject
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<GetImageStreamEventArgs>? GetImageStreamRequest;

    // Property

    [ObservableProperty]
    public partial Color LineColor { get; set; } = Colors.Black;

    [ObservableProperty]
    public partial float LineWidth { get; set; } = 5;

    public ObservableCollection<IDrawingLine> Lines { get; } = new();

    // Message

    public ValueTask<Stream?> GetImageStream(CancellationToken token = default)
    {
        var args = new GetImageStreamEventArgs
        {
            Token = token
        };
        GetImageStreamRequest?.Invoke(this, args);
        return args.Task;
    }
}
