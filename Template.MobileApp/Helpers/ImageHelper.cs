namespace Template.MobileApp.Helpers;

public static class ImageHelper
{
    public static SKBitmap ToNormalizeBitmap(Stream stream)
    {
        using var codec = SKCodec.Create(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var bitmap = SKBitmap.Decode(stream);

        SKBitmap rotated;
        switch (codec.EncodedOrigin)
        {
            case SKEncodedOrigin.BottomRight:
                using (var surface = new SKCanvas(bitmap))
                {
                    surface.RotateDegrees(180, (float)bitmap.Width / 2, (float)bitmap.Height / 2);
                    surface.DrawBitmap(bitmap.Copy(), 0, 0);
                    return bitmap;
                }
            case SKEncodedOrigin.RightTop:
                rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                using (var surface = new SKCanvas(rotated))
                {
                    surface.Translate(rotated.Width, 0);
                    surface.RotateDegrees(90);
                    surface.DrawBitmap(bitmap, 0, 0);
                    bitmap.Dispose();
                    return rotated;
                }
            case SKEncodedOrigin.LeftBottom:
                rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                using (var surface = new SKCanvas(rotated))
                {
                    surface.Translate(0, rotated.Height);
                    surface.RotateDegrees(270);
                    surface.DrawBitmap(bitmap, 0, 0);
                    bitmap.Dispose();
                    return rotated;
                }
            default:
                return bitmap;
        }
    }

    public static SKBitmap Resize(SKBitmap bitmap, double factor)
    {
        return bitmap.Resize(new SKImageInfo((int)(bitmap.Width * factor), (int)(bitmap.Height * factor)), new SKSamplingOptions(SKCubicResampler.Mitchell));
    }
}
