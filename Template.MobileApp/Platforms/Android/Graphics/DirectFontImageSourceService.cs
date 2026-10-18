#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;

using Microsoft.Maui.Platform;

internal sealed class DirectFontImageSourceService : FontImageSourceService
{
    public DirectFontImageSourceService(IFontManager fontManager)
        : base(fontManager)
    {
    }

    public override Task<IImageSourceServiceResult?> LoadDrawableAsync(IImageSource imageSource, ImageView imageView, CancellationToken cancellationToken = default)
    {
        var fontImageSource = (IFontImageSource)imageSource;
        if (fontImageSource.IsEmpty)
        {
            return Task.FromResult<IImageSourceServiceResult?>(null);
        }

        using var drawable = CreateDrawable(fontImageSource, imageView.Context!);
        imageView.SetImageDrawable(drawable);
        return Task.FromResult<IImageSourceServiceResult?>(new ImageSourceServiceLoadResult());
    }

    public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
    {
        var fontImageSource = (IFontImageSource)imageSource;
        if (fontImageSource.IsEmpty)
        {
            return Task.FromResult<IImageSourceServiceResult<Drawable>?>(null);
        }

        return Task.FromResult<IImageSourceServiceResult<Drawable>?>(new DrawableResult(CreateDrawable(fontImageSource, context)));
    }

    private BitmapDrawable CreateDrawable(IFontImageSource fontImageSource, Context context)
    {
        var unit = fontImageSource.Font.AutoScalingEnabled ? ComplexUnitType.Sp : ComplexUnitType.Dip;
        var textSize = TypedValue.ApplyDimension(unit, FontManager.GetFontSize(fontImageSource.Font).Value, context.Resources?.DisplayMetrics);
        var glyph = fontImageSource.Glyph;

        using var paint = new Paint();
        paint.TextSize = textSize;
        paint.AntiAlias = true;
        paint.Color = ToPlatformColor(fontImageSource.Color);
        paint.TextAlign = Paint.Align.Left;
        paint.SetTypeface(FontManager.GetTypeface(fontImageSource.Font));

        var width = (int)(paint.MeasureText(glyph) + .5f);
        var baseline = (int)(-paint.Ascent() + .5f);
        var height = (int)(baseline + paint.Descent() + .5f);

        using var bitmap = Bitmap.CreateBitmap(Math.Max(width, 1), Math.Max(height, 1), Bitmap.Config.Argb8888!);
        using var canvas = new Canvas(bitmap);
        canvas.DrawText(glyph, 0, baseline, paint);

        return new BitmapDrawable(context.Resources, bitmap);
    }

    private static Color ToPlatformColor(Microsoft.Maui.Graphics.Color? color) =>
        (color ?? Colors.White).ToPlatform();

    private sealed class DrawableResult : IImageSourceServiceResult<Drawable>
    {
        public Drawable Value { get; }

        public bool IsResolutionDependent => false;

        public bool IsDisposed { get; private set; }

        public DrawableResult(Drawable value)
        {
            Value = value;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Value.Dispose();
            }
        }
    }
}
