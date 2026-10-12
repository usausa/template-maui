namespace Template.MobileApp.Helpers;

public static class ImageHelper
{
    public static void ReplaceBitmap(SKBitmapImageSource source, SKBitmap? bitmap)
    {
        var old = source.Bitmap;
        source.Bitmap = bitmap!;
        if (!ReferenceEquals(old, bitmap))
        {
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            old?.Dispose();
        }
    }

    public static SKBitmap ToNormalizeBitmap(Stream stream)
    {
        using var codec = SKCodec.Create(stream);
        return codec.DecodeOriented();
    }

    public static SKBitmap Resize(SKBitmap bitmap, double factor) => bitmap.Resize((float)factor);
}
