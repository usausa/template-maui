namespace Template.MobileApp.Controls;

using SkiaSharp.Views.Maui;

using Svg.Skia;

public sealed class SvgView : SKCanvasView
{
    public static readonly BindableProperty SvgProperty = BindableProperty.Create(
        nameof(Svg),
        typeof(SKSvg),
        typeof(SvgView),
        propertyChanged: Invalidate);

    public SKSvg? Svg
    {
        get => (SKSvg?)GetValue(SvgProperty);
        set => SetValue(SvgProperty, value);
    }

    public SvgView()
    {
        PaintSurface += OnPaintSurface;
    }

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((SvgView)bindable).InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var surface = e.Surface;
        var canvas = surface.Canvas;

        if (BackgroundColor is not null)
        {
            canvas.Clear(BackgroundColor.ToSKColor());
        }

        var svg = Svg;
        if (svg?.Picture is null)
        {
            return;
        }

        var canvasMin = Math.Min(e.Info.Width, e.Info.Height);
        var svgMax = Math.Max(svg.Picture.CullRect.Width, svg.Picture.CullRect.Height);
        var scale = canvasMin / svgMax;
        var x = (e.Info.Width - (svg.Picture.CullRect.Width * scale)) / 2;
        var y = (e.Info.Height - (svg.Picture.CullRect.Height * scale)) / 2;
        var matrix = SKMatrix.CreateScale(scale, scale);

        canvas.Save();
        canvas.Translate(x, y);
        canvas.DrawPicture(svg.Picture, matrix);
        canvas.Restore();
    }
}
