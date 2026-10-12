namespace Template.MobileApp.Controls;

public sealed class CameraOverlayView : GraphicsView, IDrawable
{
    private const float CornerMargin = 16f;

    private const float CornerLength = 28f;

    private const float GridAlpha = 0.35f;

    private const float GridStrokeSize = 1f;

    private const float CornerAlpha = 0.8f;

    private const float CornerStrokeSize = 3f;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(
        nameof(ShowGrid),
        typeof(bool),
        typeof(CameraOverlayView),
        true,
        propertyChanged: HandlePropertyChanged);

    public static readonly BindableProperty ShowCornersProperty = BindableProperty.Create(
        nameof(ShowCorners),
        typeof(bool),
        typeof(CameraOverlayView),
        true,
        propertyChanged: HandlePropertyChanged);

    public static readonly BindableProperty LineColorProperty = BindableProperty.Create(
        nameof(LineColor),
        typeof(Color),
        typeof(CameraOverlayView),
        Colors.White,
        propertyChanged: HandlePropertyChanged);

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public bool ShowCorners
    {
        get => (bool)GetValue(ShowCornersProperty);
        set => SetValue(ShowCornersProperty, value);
    }

    public Color LineColor
    {
        get => (Color)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CameraOverlayView()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;
        InputTransparent = true;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((CameraOverlayView)bindable).Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var color = LineColor;

        canvas.Antialias = true;

        if (ShowGrid)
        {
            canvas.StrokeColor = new Color(color.Red, color.Green, color.Blue, GridAlpha);
            canvas.StrokeSize = GridStrokeSize;

            var w3 = dirtyRect.Width / 3f;
            var h3 = dirtyRect.Height / 3f;
            canvas.DrawLine(dirtyRect.Left + w3, dirtyRect.Top, dirtyRect.Left + w3, dirtyRect.Bottom);
            canvas.DrawLine(dirtyRect.Left + (w3 * 2), dirtyRect.Top, dirtyRect.Left + (w3 * 2), dirtyRect.Bottom);
            canvas.DrawLine(dirtyRect.Left, dirtyRect.Top + h3, dirtyRect.Right, dirtyRect.Top + h3);
            canvas.DrawLine(dirtyRect.Left, dirtyRect.Top + (h3 * 2), dirtyRect.Right, dirtyRect.Top + (h3 * 2));
        }

        if (ShowCorners)
        {
            canvas.StrokeColor = new Color(color.Red, color.Green, color.Blue, CornerAlpha);
            canvas.StrokeSize = CornerStrokeSize;
            canvas.StrokeLineCap = LineCap.Round;

            var left = dirtyRect.Left + CornerMargin;
            var top = dirtyRect.Top + CornerMargin;
            var right = dirtyRect.Right - CornerMargin;
            var bottom = dirtyRect.Bottom - CornerMargin;

            canvas.DrawLine(left, top, left + CornerLength, top);
            canvas.DrawLine(left, top, left, top + CornerLength);
            canvas.DrawLine(right - CornerLength, top, right, top);
            canvas.DrawLine(right, top, right, top + CornerLength);
            canvas.DrawLine(left, bottom, left + CornerLength, bottom);
            canvas.DrawLine(left, bottom - CornerLength, left, bottom);
            canvas.DrawLine(right - CornerLength, bottom, right, bottom);
            canvas.DrawLine(right, bottom - CornerLength, right, bottom);
        }
    }
}
