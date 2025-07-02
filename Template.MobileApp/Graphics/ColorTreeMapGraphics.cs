namespace Template.MobileApp.Graphics;

using Template.MobileApp.Usecase;

public sealed class ColorTreeMapGraphics : GraphicsObject
{
    private TreeMapNode<ColorCount>? root;

    public void Update(TreeMapNode<ColorCount>? value)
    {
        root = value;
        Invalidate();
    }

    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        if (root is not null)
        {
            DrawNode(canvas, root, dirtyRect);
        }

        canvas.RestoreState();
    }

    private static void DrawNode(ICanvas canvas, TreeMapNode<ColorCount> node, RectF rect)
    {
        if (node.IsLeaf)
        {
            canvas.FillColor = new Color(node.Value.R, node.Value.G, node.Value.B);
            canvas.StrokeSize = 1;
            canvas.FillRectangle(rect);
        }
        else
        {
            var splitVertically = rect.Width >= rect.Height;
            var ratio = node.Left.Area / node.Area;

            if (splitVertically)
            {
                var splitX = rect.X + (float)(rect.Width * ratio);
                var leftRect = new RectF(rect.X, rect.Y, splitX - rect.X, rect.Height);
                var rightRect = new RectF(splitX, rect.Y, rect.Right - splitX, rect.Height);
                DrawNode(canvas, node.Left, leftRect);
                DrawNode(canvas, node.Right, rightRect);
            }
            else
            {
                var splitY = rect.Y + (float)(rect.Height * ratio);
                var topRect = new RectF(rect.X, rect.Y, rect.Width, splitY - rect.Y);
                var bottomRect = new RectF(rect.X, splitY, rect.Width, rect.Bottom - splitY);
                DrawNode(canvas, node.Left, topRect);
                DrawNode(canvas, node.Right, bottomRect);
            }
        }
    }
}
