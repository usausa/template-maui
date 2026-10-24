namespace Template.MobileApp.Controls;

using MauiBorder = Microsoft.Maui.Controls.Border;

public sealed class PerforatedBorder : MauiBorder
{
    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty RadiusProperty = BindableProperty.Create(
        nameof(Radius),
        typeof(double),
        typeof(PerforatedBorder),
        16d,
        propertyChanged: HandleShapeChanged);

    public static readonly BindableProperty NotchRadiusProperty = BindableProperty.Create(
        nameof(NotchRadius),
        typeof(double),
        typeof(PerforatedBorder),
        12d,
        propertyChanged: HandleShapeChanged);

    public static readonly BindableProperty NotchOffsetProperty = BindableProperty.Create(
        nameof(NotchOffset),
        typeof(double),
        typeof(PerforatedBorder),
        0d,
        propertyChanged: HandleShapeChanged);

    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    public double NotchRadius
    {
        get => (double)GetValue(NotchRadiusProperty);
        set => SetValue(NotchRadiusProperty, value);
    }

    public double NotchOffset
    {
        get => (double)GetValue(NotchOffsetProperty);
        set => SetValue(NotchOffsetProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public PerforatedBorder()
    {
        UpdateShape();

        SizeChanged += (_, _) => UpdateShape();
    }

    //--------------------------------------------------------------------------------
    // Shape
    //--------------------------------------------------------------------------------

    private static void HandleShapeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue)
        {
            ((PerforatedBorder)bindable).UpdateShape();
        }
    }

    private void UpdateShape() =>
        StrokeShape = new PerforatedShape
        {
            Radius = Radius,
            NotchRadius = NotchRadius,
            NotchOffset = NotchOffset
        };

    private sealed class PerforatedShape : IShape
    {
        private const float ArcRate = 0.5522847f;

        public double Radius { get; init; }

        public double NotchRadius { get; init; }

        public double NotchOffset { get; init; }

        public PathF PathForBounds(Rect bounds)
        {
            var left = (float)bounds.Left;
            var top = (float)bounds.Top;
            var right = (float)bounds.Right;
            var bottom = (float)bounds.Bottom;

            var limit = (float)(Math.Min(bounds.Width, bounds.Height) / 2);
            var radius = Math.Clamp((float)Radius, 0f, limit);
            var notch = Math.Clamp((float)NotchRadius, 0f, limit);
            var center = NotchOffset > 0 ? top + (float)NotchOffset : top + ((float)bounds.Height / 2);
            var notchTop = Math.Clamp(center - notch, top + radius, bottom - radius);
            var notchBottom = Math.Clamp(center + notch, notchTop, bottom - radius);

            var path = new PathF();
            path.MoveTo(left + radius, top);

            path.LineTo(right - radius, top);
            Corner(path, right - radius, top, right, top, right, top + radius);

            path.LineTo(right, notchTop);
            Notch(path, right, notchTop, notchBottom, -notch);

            path.LineTo(right, bottom - radius);
            Corner(path, right, bottom - radius, right, bottom, right - radius, bottom);

            path.LineTo(left + radius, bottom);
            Corner(path, left + radius, bottom, left, bottom, left, bottom - radius);

            path.LineTo(left, notchBottom);
            Notch(path, left, notchBottom, notchTop, notch);

            path.LineTo(left, top + radius);
            Corner(path, left, top + radius, left, top, left + radius, top);

            path.Close();
            return path;
        }

        private static void Corner(PathF path, float fromX, float fromY, float cornerX, float cornerY, float toX, float toY) =>
            path.CurveTo(
                fromX + ((cornerX - fromX) * ArcRate),
                fromY + ((cornerY - fromY) * ArcRate),
                toX + ((cornerX - toX) * ArcRate),
                toY + ((cornerY - toY) * ArcRate),
                toX,
                toY);

        private static void Notch(PathF path, float edgeX, float fromY, float toY, float depth)
        {
            var arc = Math.Abs(depth) * ArcRate * Math.Sign(depth);
            var middleY = (fromY + toY) / 2;
            var reach = (toY - fromY) / 2 * ArcRate;

            path.CurveTo(edgeX + arc, fromY, edgeX + depth, middleY - reach, edgeX + depth, middleY);
            path.CurveTo(edgeX + depth, middleY + reach, edgeX + arc, toY, edgeX, toY);
        }
    }
}
