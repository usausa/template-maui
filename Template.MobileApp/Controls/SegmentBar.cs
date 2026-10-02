namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

// 段階のバー。Maximum 個の区切りのうち、Value 個を ActiveColor で塗る
public sealed class SegmentBar : SKCanvasView
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(int),
        typeof(SegmentBar),
        0,
        propertyChanged: OnVisualChanged);

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(int),
        typeof(SegmentBar),
        5,
        propertyChanged: OnVisualChanged);

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly BindableProperty ActiveColorProperty = BindableProperty.Create(
        nameof(ActiveColor),
        typeof(Color),
        typeof(SegmentBar),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public Color ActiveColor
    {
        get => (Color)GetValue(ActiveColorProperty);
        set => SetValue(ActiveColorProperty, value);
    }

    public static readonly BindableProperty InactiveColorProperty = BindableProperty.Create(
        nameof(InactiveColor),
        typeof(Color),
        typeof(SegmentBar),
        Color.FromRgba(255, 255, 255, 50),
        propertyChanged: OnVisualChanged);

    public Color InactiveColor
    {
        get => (Color)GetValue(InactiveColorProperty);
        set => SetValue(InactiveColorProperty, value);
    }

    public static readonly BindableProperty SpacingProperty = BindableProperty.Create(
        nameof(Spacing),
        typeof(double),
        typeof(SegmentBar),
        3d,
        propertyChanged: OnVisualChanged);

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((SegmentBar)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0) && (Maximum > 0))
        {
            // SKCanvas は物理ピクセルなので、論理単位の太さや長さに密度を掛ける
            var density = (float)(info.Width / Width);
            var spacing = (float)Spacing * density;
            var width = (info.Width - (spacing * (Maximum - 1))) / Maximum;
            var radius = info.Height / 2f;

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            for (var i = 0; i < Maximum; i++)
            {
                paint.Color = (i < Value ? ActiveColor : InactiveColor).ToSKColor();
                var left = i * (width + spacing);
                canvas.DrawRoundRect(new SKRect(left, 0, left + width, info.Height), radius, radius, paint);
            }
        }
    }
}
