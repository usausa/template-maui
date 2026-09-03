namespace Template.MobileApp.Graphics.Drawing;

using System.Timers;

public sealed class ColorTreeMapDrawing : DrawingObject, IDisposable
{
    private const float AnimationDuration = 500f;

    private const float LabelAreaRatio = 0.05f;

    private readonly System.Timers.Timer animationTimer = new(1000d / 60);

    private TreeMapNode<ColorCount>? root;

    private float progress = 1f;

    private long animationStart;

    public ColorTreeMapDrawing()
    {
        animationTimer.Elapsed += TimerElapsed;
    }

    public void Dispose()
    {
        animationTimer.Dispose();
    }

    public void Update(TreeMapNode<ColorCount>? value)
    {
        root = value;

        // タイルを中心からスケールインさせる
        progress = 0f;
        animationStart = Environment.TickCount64;
        animationTimer.Start();

        Invalidate();
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var elapsed = Environment.TickCount64 - animationStart;
        progress = Math.Min(1f, elapsed / AnimationDuration);
        if (progress >= 1f)
        {
            animationTimer.Stop();
        }

        SafeInvalidate();
    }

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        if (root is not null)
        {
            // CubicOut で減速しながら広がる
            var t = progress;
            var scale = 1f - ((1f - t) * (1f - t) * (1f - t));
            DrawNode(canvas, root, dirtyRect, (float)root.Area, scale, progress >= 1f);
        }

        canvas.RestoreState();
    }

    private static void DrawNode(ICanvas canvas, TreeMapNode<ColorCount> node, RectF rect, float totalArea, float scale, bool showLabel)
    {
        if (node.IsLeaf)
        {
            // タイル間 2px のギャップ+角丸
            var tileRect = rect;
            tileRect = tileRect.Inflate(-1f, -1f);

            if (scale < 1f)
            {
                var w = tileRect.Width * scale;
                var h = tileRect.Height * scale;
                tileRect = new RectF(tileRect.Center.X - (w / 2f), tileRect.Center.Y - (h / 2f), w, h);
            }

            if ((tileRect.Width <= 0f) || (tileRect.Height <= 0f))
            {
                return;
            }

            canvas.FillColor = new Color(node.Value.R, node.Value.G, node.Value.B);
            canvas.FillRoundedRectangle(tileRect, 4f);

            // 面積上位のタイルに HEX と比率を表示(輝度で文字色を自動切替)
            if (showLabel && (node.Area / totalArea >= LabelAreaRatio) && (tileRect.Width >= 72f) && (tileRect.Height >= 40f))
            {
                var luminance = ((0.299f * node.Value.R) + (0.587f * node.Value.G) + (0.114f * node.Value.B)) / 255f;
                canvas.FontColor = luminance > 0.55f ? Colors.Black : Colors.White;
                canvas.FontSize = 12f;

                var cx = tileRect.Center.X;
                var cy = tileRect.Center.Y;
                canvas.DrawString($"#{node.Value.R:X2}{node.Value.G:X2}{node.Value.B:X2}", cx, cy - 3f, HorizontalAlignment.Center);
                canvas.DrawString($"{node.Area / totalArea:P1}", cx, cy + 13f, HorizontalAlignment.Center);
            }
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
                DrawNode(canvas, node.Left, leftRect, totalArea, scale, showLabel);
                DrawNode(canvas, node.Right, rightRect, totalArea, scale, showLabel);
            }
            else
            {
                var splitY = rect.Y + (float)(rect.Height * ratio);
                var topRect = new RectF(rect.X, rect.Y, rect.Width, splitY - rect.Y);
                var bottomRect = new RectF(rect.X, splitY, rect.Width, rect.Bottom - splitY);
                DrawNode(canvas, node.Left, topRect, totalArea, scale, showLabel);
                DrawNode(canvas, node.Right, bottomRect, totalArea, scale, showLabel);
            }
        }
    }
}
