namespace Template.MobileApp.Messaging;

public enum ScrollRequestKind
{
    Offset,
    Index
}

public sealed class ScrollRequestEventArgs : EventArgs
{
    public ScrollRequestKind Kind { get; init; }

    public double X { get; init; }

    public double Y { get; init; }
}

public sealed class ScrollController
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<ScrollRequestEventArgs>? ScrollRequest;

    // Message

    public void ScrollTo(double x, double y) =>
        ScrollRequest?.Invoke(this, new ScrollRequestEventArgs { Kind = ScrollRequestKind.Offset, X = x, Y = y });

    public void ScrollTo(int index) =>
        ScrollRequest?.Invoke(this, new ScrollRequestEventArgs { Kind = ScrollRequestKind.Index, X = index });
}
