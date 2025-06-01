namespace Template.MobileApp.Graphics;

public interface IGraphicsObject : IDrawable
{
    event EventHandler<EventArgs>? InvalidateRequest;
}
