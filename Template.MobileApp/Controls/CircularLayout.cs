namespace Template.MobileApp.Controls;

using Microsoft.Maui.Layouts;

// 子要素を円周上に配置するカスタムレイアウト。
// ILayoutManager の Measure / ArrangeChildren を実装する最小例 (AlohaKit.Layouts の CircularLayout 相当)。
// StartAngle / SweepAngle で円弧配置にもなる (Nova.Avalonia.UI の ArcPanel 相当。既定値は従来どおりの全周)
public sealed class CircularLayout : Layout
{
    // 円の半径。負値なら利用可能領域から自動算出
    public static readonly BindableProperty RadiusProperty = BindableProperty.Create(
        nameof(Radius),
        typeof(double),
        typeof(CircularLayout),
        -1d,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    // 配置開始角 (度)。-90 で真上、時計回りに増加
    public static readonly BindableProperty StartAngleProperty = BindableProperty.Create(
        nameof(StartAngle),
        typeof(double),
        typeof(CircularLayout),
        -90d,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    // 配置する角度の幅 (度)。360 で全周、それ未満で円弧
    public static readonly BindableProperty SweepAngleProperty = BindableProperty.Create(
        nameof(SweepAngle),
        typeof(double),
        typeof(CircularLayout),
        360d,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    // 円弧のとき両端を含めて均等配置する (false なら SweepAngle / 子数 の刻みで終端を含めない)
    public static readonly BindableProperty DistributeEvenlyProperty = BindableProperty.Create(
        nameof(DistributeEvenly),
        typeof(bool),
        typeof(CircularLayout),
        true,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    // 円弧の外接矩形に自身のサイズを詰める (Radius 指定時のみ有効。半円メニューなどで余白を無くす)
    public static readonly BindableProperty FitToArcProperty = BindableProperty.Create(
        nameof(FitToArc),
        typeof(bool),
        typeof(CircularLayout),
        false,
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

    public double StartAngle
    {
        get => (double)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public double SweepAngle
    {
        get => (double)GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    public bool DistributeEvenly
    {
        get => (bool)GetValue(DistributeEvenlyProperty);
        set => SetValue(DistributeEvenlyProperty, value);
    }

    public bool FitToArc
    {
        get => (bool)GetValue(FitToArcProperty);
        set => SetValue(FitToArcProperty, value);
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
            var children = new List<IView>();
            var maxChild = 0d;
            foreach (var child in layout)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                var size = child.Measure(Double.PositiveInfinity, Double.PositiveInfinity);
                children.Add(child);
                maxChild = Math.Max(maxChild, Math.Max(size.Width, size.Height));
            }

            if (layout.Radius > 0d)
            {
                if (layout.FitToArc && (children.Count > 0))
                {
                    var extent = ComputeExtent(ComputeAngles(children), layout.Radius, maxChild);
                    return new Size(
                        extent.Width + layout.Padding.HorizontalThickness,
                        extent.Height + layout.Padding.VerticalThickness);
                }

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
            var angles = ComputeAngles(children);

            // 中心は領域の中央。FitToArc のときは円弧の外接矩形が中央に来るよう中心をずらす
            var cx = bounds.Center.X;
            var cy = bounds.Center.Y;
            if (layout.FitToArc && (layout.Radius > 0d))
            {
                var extent = ComputeExtent(angles, radius, maxChild);
                cx -= extent.X + (extent.Width / 2d);
                cy -= extent.Y + (extent.Height / 2d);
            }

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var rad = angles[i] * Math.PI / 180d;
                var size = child.DesiredSize;
                var x = cx + (radius * Math.Cos(rad)) - (size.Width / 2d);
                var y = cy + (radius * Math.Sin(rad)) - (size.Height / 2d);
                child.Arrange(new Rect(x, y, size.Width, size.Height));
            }

            return bounds.Size;
        }

        // 各子の角度。Angle 添付プロパティ指定の子はその値、未指定の子は StartAngle から SweepAngle を等分する
        private double[] ComputeAngles(List<IView> children)
        {
            var count = children.Count;
            var sweep = layout.SweepAngle;
            var full = sweep >= 360d;
            var step = full || !layout.DistributeEvenly
                ? sweep / count
                : (count > 1 ? sweep / (count - 1) : 0d);

            var angles = new double[count];
            for (var i = 0; i < count; i++)
            {
                var angle = GetAngle((BindableObject)children[i]);
                angles[i] = Double.IsNaN(angle) ? layout.StartAngle + (step * i) : angle;
            }

            return angles;
        }

        // 円の中心を原点とした、全ての子を含む外接矩形
        private static Rect ComputeExtent(double[] angles, double radius, double maxChild)
        {
            var minX = Double.PositiveInfinity;
            var maxX = Double.NegativeInfinity;
            var minY = Double.PositiveInfinity;
            var maxY = Double.NegativeInfinity;
            var half = maxChild / 2d;
            foreach (var angle in angles)
            {
                var rad = angle * Math.PI / 180d;
                var x = radius * Math.Cos(rad);
                var y = radius * Math.Sin(rad);
                minX = Math.Min(minX, x - half);
                maxX = Math.Max(maxX, x + half);
                minY = Math.Min(minY, y - half);
                maxY = Math.Max(maxY, y + half);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
