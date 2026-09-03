namespace Template.MobileApp.Controls;

// カメラプレビューに重ねる撮影ガイド(三分割グリッド+四隅マーカー)
public sealed class CameraOverlayView : GraphicsView, IDrawable
{
    private const float CornerMargin = 16f;
    private const float CornerLength = 28f;

    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(
        nameof(ShowGrid),
        typeof(bool),
        typeof(CameraOverlayView),
        true,
        propertyChanged: HandlePropertyChanged);

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public static readonly BindableProperty ShowCornersProperty = BindableProperty.Create(
        nameof(ShowCorners),
        typeof(bool),
        typeof(CameraOverlayView),
        true,
        propertyChanged: HandlePropertyChanged);

    public bool ShowCorners
    {
        get => (bool)GetValue(ShowCornersProperty);
        set => SetValue(ShowCornersProperty, value);
    }

    public static readonly BindableProperty LineColorProperty = BindableProperty.Create(
        nameof(LineColor),
        typeof(Color),
        typeof(CameraOverlayView),
        Colors.White,
        propertyChanged: HandlePropertyChanged);

    public Color LineColor
    {
        get => (Color)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    public CameraOverlayView()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;
        InputTransparent = true;
    }

    private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((CameraOverlayView)bindable).Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var color = LineColor;

        canvas.Antialias = true;

        // 三分割グリッド
        if (ShowGrid)
        {
            canvas.StrokeColor = new Color(color.Red, color.Green, color.Blue, 0.35f);
            canvas.StrokeSize = 1f;

            var w3 = dirtyRect.Width / 3f;
            var h3 = dirtyRect.Height / 3f;
            canvas.DrawLine(dirtyRect.Left + w3, dirtyRect.Top, dirtyRect.Left + w3, dirtyRect.Bottom);
            canvas.DrawLine(dirtyRect.Left + (w3 * 2), dirtyRect.Top, dirtyRect.Left + (w3 * 2), dirtyRect.Bottom);
            canvas.DrawLine(dirtyRect.Left, dirtyRect.Top + h3, dirtyRect.Right, dirtyRect.Top + h3);
            canvas.DrawLine(dirtyRect.Left, dirtyRect.Top + (h3 * 2), dirtyRect.Right, dirtyRect.Top + (h3 * 2));
        }

        // 四隅のコーナーマーカー
        if (ShowCorners)
        {
            canvas.StrokeColor = new Color(color.Red, color.Green, color.Blue, 0.8f);
            canvas.StrokeSize = 3f;
            canvas.StrokeLineCap = LineCap.Round;

            var left = dirtyRect.Left + CornerMargin;
            var top = dirtyRect.Top + CornerMargin;
            var right = dirtyRect.Right - CornerMargin;
            var bottom = dirtyRect.Bottom - CornerMargin;

            // 左上
            canvas.DrawLine(left, top, left + CornerLength, top);
            canvas.DrawLine(left, top, left, top + CornerLength);
            // 右上
            canvas.DrawLine(right - CornerLength, top, right, top);
            canvas.DrawLine(right, top, right, top + CornerLength);
            // 左下
            canvas.DrawLine(left, bottom, left + CornerLength, bottom);
            canvas.DrawLine(left, bottom - CornerLength, left, bottom);
            // 右下
            canvas.DrawLine(right - CornerLength, bottom, right, bottom);
            canvas.DrawLine(right, bottom - CornerLength, right, bottom);
        }
    }
}
