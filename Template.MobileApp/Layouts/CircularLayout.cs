namespace Template.MobileApp.Layouts;

using Microsoft.Maui.Layouts;

// 子要素を円周上に配置するカスタムレイアウト。
// ILayoutManager の Measure / ArrangeChildren を実装する最小例 (AlohaKit.Layouts の CircularLayout 相当)
public sealed class CircularLayout : Layout
{
    // 円の半径。負値なら利用可能領域から自動算出
    public static readonly BindableProperty RadiusProperty = BindableProperty.Create(
        nameof(Radius),
        typeof(double),
        typeof(CircularLayout),
        -1d,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    // 子要素ごとの角度 (度)。未指定 (NaN) の子は均等配置 (真上開始・時計回り)
    public static readonly BindableProperty AngleProperty = BindableProperty.CreateAttached(
        "Angle",
        typeof(double),
        typeof(CircularLayout),
        Double.NaN);

    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    public static double GetAngle(BindableObject bindable) => (double)bindable.GetValue(AngleProperty);

    public static void SetAngle(BindableObject bindable, double value) => bindable.SetValue(AngleProperty, value);

    protected override ILayoutManager CreateLayoutManager() => new CircularLayoutManager(this);

    private sealed class CircularLayoutManager : ILayoutManager
    {
        private readonly CircularLayout layout;

        public CircularLayoutManager(CircularLayout layout)
        {
            this.layout = layout;
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var maxChild = 0d;
            foreach (var child in layout)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                var size = child.Measure(Double.PositiveInfinity, Double.PositiveInfinity);
                maxChild = Math.Max(maxChild, Math.Max(size.Width, size.Height));
            }

            if (layout.Radius > 0d)
            {
                var side = ((layout.Radius + (maxChild / 2d)) * 2d) + layout.Padding.HorizontalThickness;
                return new Size(side, side);
            }

            var width = Double.IsFinite(widthConstraint) ? widthConstraint : maxChild * 4d;
            var height = Double.IsFinite(heightConstraint) ? heightConstraint : width;
            var side2 = Math.Min(width, height);
            return new Size(side2, side2);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var children = new List<IView>();
            var maxChild = 0d;
            foreach (var child in layout)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                children.Add(child);
                maxChild = Math.Max(maxChild, Math.Max(child.DesiredSize.Width, child.DesiredSize.Height));
            }

            if (children.Count == 0)
            {
                return bounds.Size;
            }

            var radius = layout.Radius > 0d
                ? layout.Radius
                : (Math.Min(bounds.Width, bounds.Height) / 2d) - (maxChild / 2d);
            var cx = bounds.Center.X;
            var cy = bounds.Center.Y;

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var angle = GetAngle((BindableObject)child);
                if (Double.IsNaN(angle))
                {
                    angle = -90d + (360d * i / children.Count);
                }

                var rad = angle * Math.PI / 180d;
                var size = child.DesiredSize;
                var x = cx + (radius * Math.Cos(rad)) - (size.Width / 2d);
                var y = cy + (radius * Math.Sin(rad)) - (size.Height / 2d);
                child.Arrange(new Rect(x, y, size.Width, size.Height));
            }

            return bounds.Size;
        }
    }
}
