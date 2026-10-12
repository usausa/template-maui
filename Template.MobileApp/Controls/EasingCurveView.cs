namespace Template.MobileApp.Controls;

public sealed class EasingCurveView : GraphicsView, IDrawable
{
    private const float BallRadius = 8f;

    private const float MarginRate = 0.18f;

    private const int CurveSegments = 40;

    private const float GuideStrokeSize = 1f;

    private const float GuideDash = 4f;

    private const float CurveStrokeSize = 2f;

    private static readonly Color GuideColor = Color.FromArgb("#E0E0E0");

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty EasingProperty = BindableProperty.Create(
        nameof(Easing),
        typeof(Easing),
        typeof(EasingCurveView),
        Easing.Linear,
        propertyChanged: Invalidate);

    public static readonly BindableProperty LineColorProperty = BindableProperty.Create(
        nameof(LineColor),
        typeof(Color),
        typeof(EasingCurveView),
        Color.FromArgb("#90CAF9"),
        propertyChanged: Invalidate);

    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
        nameof(Progress),
        typeof(double),
        typeof(EasingCurveView),
        0d,
        propertyChanged: Invalidate);

    public Easing Easing
    {
        get => (Easing)GetValue(EasingProperty);
        set => SetValue(EasingProperty, value);
    }

    public Color LineColor
    {
        get => (Color)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public EasingCurveView()
    {
        Drawable = this;
        InputTransparent = true;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((EasingCurveView)bindable).Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        if ((width <= 0) || (height <= 0))
        {
            return;
        }

        var usable = height * (1f - (MarginRate * 2f));

        var bottom = dirtyRect.Bottom - (height * MarginRate);
        var top = bottom - usable;
        canvas.StrokeColor = GuideColor;
        canvas.StrokeSize = GuideStrokeSize;
        canvas.StrokeDashPattern = [GuideDash, GuideDash];
        canvas.DrawLine(dirtyRect.Left, bottom, dirtyRect.Right, bottom);
        canvas.DrawLine(dirtyRect.Left, top, dirtyRect.Right, top);
        canvas.StrokeDashPattern = null;

        var left = dirtyRect.Left + BallRadius;
        var span = width - (BallRadius * 2f);

        using var path = new PathF();
        for (var i = 0; i <= CurveSegments; i++)
        {
            var t = i / (float)CurveSegments;
            var v = (float)Easing.Ease(t);
            var x = left + (t * span);
            var y = bottom - (v * usable);
            if (i == 0)
            {
                path.MoveTo(x, y);
            }
            else
            {
                path.LineTo(x, y);
            }
        }

        canvas.StrokeColor = LineColor;
        canvas.StrokeSize = CurveStrokeSize;
        canvas.DrawPath(path);

        var progress = (float)Math.Clamp(Progress, 0d, 1d);
        canvas.FillColor = LineColor;
        canvas.FillCircle(left + (progress * span), bottom - ((float)Easing.Ease(progress) * usable), BallRadius);
    }
}
