namespace Template.MobileApp.Graphics.Drawing;

using IImage = Microsoft.Maui.Graphics.IImage;

// 画像トリミング (C-6)。枠のドラッグ移動と四隅ハンドルのリサイズに対応し、
// 書き出しは表示と同じ OnDraw を使う (ExportPng 共用構成)
public sealed class CropDrawing : DrawingObject, IInteractiveDrawing, IDisposable
{
    private const float Padding = 12f;
    private const float HandleRadius = 10f;
    private const float HandleHitRadius = 28f;
    private const float MinSize = 64f;
    private const int MaxExportSize = 4096;

    private static readonly Color BackgroundColor = Color.FromArgb("#263238");
    private static readonly Color DimColor = Color.FromRgba(0, 0, 0, 140);
    private static readonly Color AccentColor = Color.FromArgb("#2196F3");

    private enum DragMode
    {
        None,
        Move,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private IImage? image;

    private RectF imageRect;
    private RectF cropRect;
    private bool cropInitialized;
    private bool exporting;

    private DragMode dragMode;
    private PointF lastPoint;

    public void SetImage(IImage? value)
    {
        image?.Dispose();
        image = value;
        cropInitialized = false;
        Invalidate();
    }

    public void Reset()
    {
        cropInitialized = false;
        Invalidate();
    }

    public void Dispose()
    {
        image?.Dispose();
        image = null;
    }

    // 表示中の枠を画像ピクセルへ換算して PNG 書き出しする
    public (int Width, int Height) ExportCrop(Stream stream)
    {
        if ((image is null) || !cropInitialized || (imageRect.Width <= 0))
        {
            return (0, 0);
        }

        var scale = image.Width / imageRect.Width;
        var width = Math.Clamp((int)MathF.Round(cropRect.Width * scale), 1, MaxExportSize);
        var height = Math.Clamp((int)MathF.Round(cropRect.Height * scale), 1, MaxExportSize);

        exporting = true;
        try
        {
            ExportPng(stream, width, height);
        }
        finally
        {
            exporting = false;
        }

        return (width, height);
    }

    //--------------------------------------------------------------------------------
    // Interaction
    //--------------------------------------------------------------------------------

    public void OnInteractionStart(PointF point)
    {
        if (!cropInitialized)
        {
            return;
        }

        dragMode = HitTest(point);
        lastPoint = point;
    }

    public void OnInteractionDrag(PointF point)
    {
        if (dragMode == DragMode.None)
        {
            return;
        }

        var dx = point.X - lastPoint.X;
        var dy = point.Y - lastPoint.Y;
        lastPoint = point;

        var left = cropRect.Left;
        var top = cropRect.Top;
        var right = cropRect.Right;
        var bottom = cropRect.Bottom;

        switch (dragMode)
        {
            case DragMode.Move:
                var x = Math.Clamp(cropRect.X + dx, imageRect.Left, imageRect.Right - cropRect.Width);
                var y = Math.Clamp(cropRect.Y + dy, imageRect.Top, imageRect.Bottom - cropRect.Height);
                cropRect = new RectF(x, y, cropRect.Width, cropRect.Height);
                break;
            case DragMode.TopLeft:
                left = Math.Clamp(left + dx, imageRect.Left, right - MinSize);
                top = Math.Clamp(top + dy, imageRect.Top, bottom - MinSize);
                cropRect = new RectF(left, top, right - left, bottom - top);
                break;
            case DragMode.TopRight:
                right = Math.Clamp(right + dx, left + MinSize, imageRect.Right);
                top = Math.Clamp(top + dy, imageRect.Top, bottom - MinSize);
                cropRect = new RectF(left, top, right - left, bottom - top);
                break;
            case DragMode.BottomLeft:
                left = Math.Clamp(left + dx, imageRect.Left, right - MinSize);
                bottom = Math.Clamp(bottom + dy, top + MinSize, imageRect.Bottom);
                cropRect = new RectF(left, top, right - left, bottom - top);
                break;
            case DragMode.BottomRight:
                right = Math.Clamp(right + dx, left + MinSize, imageRect.Right);
                bottom = Math.Clamp(bottom + dy, top + MinSize, imageRect.Bottom);
                cropRect = new RectF(left, top, right - left, bottom - top);
                break;
        }

        Invalidate();
    }

    public void OnInteractionEnd(PointF point)
    {
        dragMode = DragMode.None;
    }

    private DragMode HitTest(PointF point)
    {
        if (Distance(point, new PointF(cropRect.Left, cropRect.Top)) <= HandleHitRadius)
        {
            return DragMode.TopLeft;
        }
        if (Distance(point, new PointF(cropRect.Right, cropRect.Top)) <= HandleHitRadius)
        {
            return DragMode.TopRight;
        }
        if (Distance(point, new PointF(cropRect.Left, cropRect.Bottom)) <= HandleHitRadius)
        {
            return DragMode.BottomLeft;
        }
        if (Distance(point, new PointF(cropRect.Right, cropRect.Bottom)) <= HandleHitRadius)
        {
            return DragMode.BottomRight;
        }
        return cropRect.Contains(point) ? DragMode.Move : DragMode.None;
    }

    private static float Distance(PointF a, PointF b) =>
        MathF.Sqrt(((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));

    //--------------------------------------------------------------------------------
    // Draw
    //--------------------------------------------------------------------------------

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        if (exporting)
        {
            DrawExport(canvas, dirtyRect);
            return;
        }

        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle(dirtyRect);

        if ((image is null) || (dirtyRect.Width < MinSize) || (dirtyRect.Height < MinSize))
        {
            return;
        }

        canvas.Antialias = true;

        imageRect = CalculateImageRect(dirtyRect);
        if (!cropInitialized)
        {
            cropRect = new RectF(
                imageRect.X + (imageRect.Width * 0.15f),
                imageRect.Y + (imageRect.Height * 0.15f),
                imageRect.Width * 0.7f,
                imageRect.Height * 0.7f);
            cropInitialized = true;
        }
        else
        {
            // リレイアウトで画像枠が変わった場合に収まるよう寄せる
            cropRect = ClampToImage(cropRect);
        }

        canvas.DrawImage(image, imageRect.X, imageRect.Y, imageRect.Width, imageRect.Height);

        // 枠外の減光
        canvas.FillColor = DimColor;
        canvas.FillRectangle(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width, cropRect.Top - dirtyRect.Top);
        canvas.FillRectangle(dirtyRect.Left, cropRect.Top, cropRect.Left - dirtyRect.Left, cropRect.Height);
        canvas.FillRectangle(cropRect.Right, cropRect.Top, dirtyRect.Right - cropRect.Right, cropRect.Height);
        canvas.FillRectangle(dirtyRect.Left, cropRect.Bottom, dirtyRect.Width, dirtyRect.Bottom - cropRect.Bottom);

        // トリミング枠と三分割線
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 2f;
        canvas.DrawRectangle(cropRect);

        canvas.StrokeColor = Colors.White.WithAlpha(0.4f);
        canvas.StrokeSize = 1f;
        for (var i = 1; i <= 2; i++)
        {
            var x = cropRect.Left + (cropRect.Width * i / 3f);
            var y = cropRect.Top + (cropRect.Height * i / 3f);
            canvas.DrawLine(x, cropRect.Top, x, cropRect.Bottom);
            canvas.DrawLine(cropRect.Left, y, cropRect.Right, y);
        }

        // 四隅のハンドル
        canvas.FillColor = Colors.White;
        canvas.StrokeColor = AccentColor;
        canvas.StrokeSize = 2f;
        DrawHandle(canvas, cropRect.Left, cropRect.Top);
        DrawHandle(canvas, cropRect.Right, cropRect.Top);
        DrawHandle(canvas, cropRect.Left, cropRect.Bottom);
        DrawHandle(canvas, cropRect.Right, cropRect.Bottom);
    }

    private static void DrawHandle(ICanvas canvas, float x, float y)
    {
        canvas.FillCircle(x, y, HandleRadius);
        canvas.DrawCircle(x, y, HandleRadius);
    }

    private void DrawExport(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        if ((image is null) || (cropRect.Width <= 0))
        {
            return;
        }

        // 枠内が出力全体になるよう画像全体を拡大配置する
        var scale = dirtyRect.Width / cropRect.Width;
        canvas.DrawImage(
            image,
            (imageRect.X - cropRect.X) * scale,
            (imageRect.Y - cropRect.Y) * scale,
            imageRect.Width * scale,
            imageRect.Height * scale);
    }

    private RectF CalculateImageRect(RectF dirtyRect)
    {
        var availableWidth = dirtyRect.Width - (Padding * 2);
        var availableHeight = dirtyRect.Height - (Padding * 2);
        var scale = Math.Min(availableWidth / image!.Width, availableHeight / image.Height);
        var width = image.Width * scale;
        var height = image.Height * scale;
        return new RectF(
            dirtyRect.Left + ((dirtyRect.Width - width) / 2),
            dirtyRect.Top + ((dirtyRect.Height - height) / 2),
            width,
            height);
    }

    private RectF ClampToImage(RectF rect)
    {
        var width = Math.Min(rect.Width, imageRect.Width);
        var height = Math.Min(rect.Height, imageRect.Height);
        var x = Math.Clamp(rect.X, imageRect.Left, imageRect.Right - width);
        var y = Math.Clamp(rect.Y, imageRect.Top, imageRect.Bottom - height);
        return new RectF(x, y, width, height);
    }
}
