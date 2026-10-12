namespace Template.MobileApp.Controls;

using Android.Graphics;

using SkiaSharp.Views.Android;

public sealed partial class HomeBlurredImage
{
    //--------------------------------------------------------------------------------
    // Platform
    //--------------------------------------------------------------------------------

    private static partial SKBitmap? LoadBitmap(string? source)
    {
        var context = Platform.AppContext;
        var id = String.IsNullOrEmpty(source) ? 0 : context.Resources!.GetIdentifier(System.IO.Path.GetFileNameWithoutExtension(source), "drawable", context.PackageName);
        if (id == 0)
        {
            return null;
        }

        using var options = new BitmapFactory.Options();
        options.InScaled = false;
        using var bitmap = BitmapFactory.DecodeResource(context.Resources, id, options);
        return bitmap?.ToSKBitmap();
    }
}
