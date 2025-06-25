namespace Template.MobileApp.Graphics;

using BarcodeScanning;

public sealed class BarcodeGraphics : GraphicsObject
{
    private IReadOnlySet<BarcodeResult>? results;

    public void Update(IReadOnlySet<BarcodeResult>? values)
    {
        results = values;
        Invalidate();
    }

    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (results is not null && results.Count > 0)
        {
            var scale = 1 / canvas.DisplayScale;
            canvas.Scale(scale, scale);

            canvas.StrokeSize = 15;
            canvas.StrokeColor = Colors.Red;

            foreach (var barcode in results)
            {
                canvas.DrawRectangle(barcode.PreviewBoundingBox);
            }
        }
    }
}
