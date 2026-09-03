namespace Template.MobileApp.Controls;

// Easing 曲線を背景に描画する(横=時間 0→1、縦=出力 0→1。Spring/Bounce のオーバーシュートに備えて上下に余白を取る)
public sealed class EasingCurveView : GraphicsView, IDrawable
{
    public static readonly BindableProperty EasingProperty = BindableProperty.Create(
        nameof(Easing),
        typeof(Easing),
        typeof(EasingCurveView),
        Easing.Linear,
        propertyChanged: Invalidate);

    public Easing Easing
    {
        get => (Easing)GetValue(EasingProperty);
        set => SetValue(EasingProperty, value);
    }

    // 既定値は BlueLighten3 相当
    public static readonly BindableProperty LineColorProperty = BindableProperty.Create(
        nameof(LineColor),
        typeof(Color),
        typeof(EasingCurveView),
        Color.FromArgb("#90CAF9"),
        propertyChanged: Invalidate);

    public Color LineColor
    {
        get => (Color)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    public EasingCurveView()
    {
        Drawable = this;
        InputTransparent = true;
    }

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

        const float margin = 0.18f;
        var usable = height * (1f - (margin * 2f));

        using var path = new PathF();
        for (var i = 0; i <= 40; i++)
        {
            var t = i / 40f;
            var v = (float)Easing.Ease(t);
            var x = dirtyRect.Left + (t * width);
            var y = dirtyRect.Bottom - (height * margin) - (v * usable);
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
        canvas.StrokeSize = 2f;
        canvas.DrawPath(path);
    }
}
