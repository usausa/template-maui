namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

// 横の一覧の 1 項目ぶんの折れ線。中央に自分の値の点を置き、左右の端は前後の値との中間まで線を引く。
// 高さは全体の範囲 (Minimum〜Maximum) に合わせるので、項目の間を空けずに並べると一覧をまたいで線がつながる
public sealed class SeriesSegmentView : SKCanvasView
{
    private static readonly SKTypeface BoldTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(SeriesSegmentView),
        0d,
        propertyChanged: OnVisualChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // 前の項目の値 (先頭は null)
    public static readonly BindableProperty PreviousProperty = BindableProperty.Create(
        nameof(Previous),
        typeof(double?),
        typeof(SeriesSegmentView),
        propertyChanged: OnVisualChanged);

    public double? Previous
    {
        get => (double?)GetValue(PreviousProperty);
        set => SetValue(PreviousProperty, value);
    }

    // 次の項目の値 (末尾は null)
    public static readonly BindableProperty NextProperty = BindableProperty.Create(
        nameof(Next),
        typeof(double?),
        typeof(SeriesSegmentView),
        propertyChanged: OnVisualChanged);

    public double? Next
    {
        get => (double?)GetValue(NextProperty);
        set => SetValue(NextProperty, value);
    }

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(SeriesSegmentView),
        0d,
        propertyChanged: OnVisualChanged);

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(SeriesSegmentView),
        1d,
        propertyChanged: OnVisualChanged);

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // 点の上に出す文字
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(SeriesSegmentView),
        propertyChanged: OnVisualChanged);

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(SeriesSegmentView),
        14d,
        propertyChanged: OnVisualChanged);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(SeriesSegmentView),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty LineColorProperty = BindableProperty.Create(
        nameof(LineColor),
        typeof(Color),
        typeof(SeriesSegmentView),
        Colors.White,
        propertyChanged: OnVisualChanged);

    public Color LineColor
    {
        get => (Color)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    // 線の下の面 (下に向かって透明になる)
    public static readonly BindableProperty FillColorProperty = BindableProperty.Create(
        nameof(FillColor),
        typeof(Color),
        typeof(SeriesSegmentView),
        Color.FromRgba(255, 255, 255, 64),
        propertyChanged: OnVisualChanged);

    public Color FillColor
    {
        get => (Color)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    // 点を大きくして光らせる (今の項目など)
    public static readonly BindableProperty IsHighlightedProperty = BindableProperty.Create(
        nameof(IsHighlighted),
        typeof(bool),
        typeof(SeriesSegmentView),
        false,
        propertyChanged: OnVisualChanged);

    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((SeriesSegmentView)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            // SKCanvas は物理ピクセルなので、論理単位の太さや長さに密度を掛ける
            var density = (float)(info.Width / Width);
            using var font = new SKFont(BoldTypeface, (float)FontSize * density);

            // 上は文字、下は点の半径のぶんを空ける
            var top = (font.Metrics.Descent - font.Metrics.Ascent) + (12 * density);
            var bottom = info.Height - (8 * density);
            var center = new SKPoint(info.Width / 2f, ToY(Value, top, bottom));
            var left = new SKPoint(0, Previous is { } previous ? ToY((previous + Value) / 2, top, bottom) : center.Y);
            var right = new SKPoint(info.Width, Next is { } next ? ToY((Value + next) / 2, top, bottom) : center.Y);
            var start = Previous is null ? center : left;
            var end = Next is null ? center : right;

            DrawFill(canvas, start, center, end, top, info.Height);
            DrawLine(canvas, start, center, end, density);
            DrawPoint(canvas, center, density);
            DrawText(canvas, center, font, density);
        }
    }

    private float ToY(double value, float top, float bottom)
    {
        var range = Maximum - Minimum;
        var ratio = range > 0 ? Math.Clamp((value - Minimum) / range, 0, 1) : 0.5;
        return bottom - (float)(ratio * (bottom - top));
    }

    // 面のグラデーションは項目の高さで決めるので、隣の項目と同じ濃さでつながる
    private void DrawFill(SKCanvas canvas, SKPoint start, SKPoint center, SKPoint end, float top, float height)
    {
        using var builder = new SKPathBuilder();
        builder.MoveTo(start);
        builder.LineTo(center);
        builder.LineTo(end);
        builder.LineTo(end.X, height);
        builder.LineTo(start.X, height);
        builder.Close();
        using var path = builder.Detach();

        var color = FillColor.ToSKColor();
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, top),
            new SKPoint(0, height),
            [color, color.WithAlpha(0)],
            SKShaderTileMode.Clamp);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Shader = shader;
        canvas.DrawPath(path, paint);
    }

    private void DrawLine(SKCanvas canvas, SKPoint start, SKPoint center, SKPoint end, float density)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 2 * density;
        paint.Color = LineColor.ToSKColor();
        canvas.DrawLine(start, center, paint);
        canvas.DrawLine(center, end, paint);
    }

    private void DrawPoint(SKCanvas canvas, SKPoint center, float density)
    {
        var color = LineColor.ToSKColor();
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        if (IsHighlighted)
        {
            paint.Color = color.WithAlpha(90);
            canvas.DrawCircle(center, 8 * density, paint);
        }

        paint.Color = color;
        canvas.DrawCircle(center, (IsHighlighted ? 4.5f : 3f) * density, paint);
    }

    private void DrawText(SKCanvas canvas, SKPoint center, SKFont font, float density)
    {
        if (!String.IsNullOrEmpty(Text))
        {
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Color = TextColor.ToSKColor();
            canvas.DrawText(Text, center.X, center.Y - (10 * density) - font.Metrics.Descent, SKTextAlign.Center, font, paint);
        }
    }
}
