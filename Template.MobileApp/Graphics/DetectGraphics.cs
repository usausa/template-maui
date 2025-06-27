namespace Template.MobileApp.Graphics;

using Template.MobileApp.Usecase;

public sealed class DetectGraphics : GraphicsObject
{
    private float width;

    private float height;

    private DetectResult[] results = [];

    public void Update(float w, float h, DetectResult[] values)
    {
        width = w;
        height = h;
        results = values;
        Invalidate();
    }

    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if ((width == 0) || (height == 0) || (results.Length == 0))
        {
            return;
        }

        var layoutWidth = dirtyRect.Width;
        var layoutHeight = dirtyRect.Height;

        var scale = Math.Min(layoutWidth / width, layoutHeight / height);

        var displayWidth = width * scale;
        var displayHeight = height * scale;

        var offsetX = (layoutWidth - displayWidth) / 2;
        var offsetY = (layoutHeight - displayHeight) / 2;
        var imageRect = new RectF(offsetX, offsetY, displayWidth, displayHeight);

        canvas.Antialias = true;
        canvas.StrokeSize = 5;
        canvas.FontSize = 16f;
        canvas.FontColor = Colors.White;

        foreach (var result in results)
        {
            var x = imageRect.X + (result.Left * imageRect.Width);
            var y = imageRect.Y + (result.Top * imageRect.Height);
            var w = (result.Right - result.Left) * imageRect.Width;
            var h = (result.Bottom - result.Top) * imageRect.Height;

            var c = (byte)(255 * (1 - result.Score));
            canvas.StrokeColor = new Color((byte)255, c, (byte)0, (byte)255);
            canvas.DrawRectangle(x, y, w, h);

            canvas.DrawString($"{result.Score:F2}", x, y, w, h, HorizontalAlignment.Right, VerticalAlignment.Bottom);
        }
    }
}
