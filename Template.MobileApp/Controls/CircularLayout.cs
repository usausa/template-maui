namespace Template.MobileApp.Controls;

using Microsoft.Maui.Layouts;

public sealed class CircularLayout : Layout
{
    private const double UnboundedSizeRate = 4;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty RadiusProperty = BindableProperty.Create(
        nameof(Radius),
        typeof(double),
        typeof(CircularLayout),
        -1d,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    public static readonly BindableProperty StartAngleProperty = BindableProperty.Create(
        nameof(StartAngle),
        typeof(double),
        typeof(CircularLayout),
        -90d,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    public static readonly BindableProperty SweepAngleProperty = BindableProperty.Create(
        nameof(SweepAngle),
        typeof(double),
        typeof(CircularLayout),
        360d,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    public static readonly BindableProperty DistributeEvenlyProperty = BindableProperty.Create(
        nameof(DistributeEvenly),
        typeof(bool),
        typeof(CircularLayout),
        true,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    public static readonly BindableProperty FitToArcProperty = BindableProperty.Create(
        nameof(FitToArc),
        typeof(bool),
        typeof(CircularLayout),
        false,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    public static readonly BindableProperty RotateItemsProperty = BindableProperty.Create(
        nameof(RotateItems),
        typeof(bool),
        typeof(CircularLayout),
        false,
        propertyChanged: static (bindable, _, newValue) => ((CircularLayout)bindable).OnRotateItemsChanged((bool)newValue));

    public static readonly BindableProperty ItemAngleProperty = BindableProperty.Create(
        nameof(ItemAngle),
        typeof(double),
        typeof(CircularLayout),
        0d,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    public static readonly BindableProperty OrbitSpacingProperty = BindableProperty.Create(
        nameof(OrbitSpacing),
        typeof(double),
        typeof(CircularLayout),
        48d,
        propertyChanged: static (bindable, _, _) => ((CircularLayout)bindable).InvalidateMeasure());

    public static readonly BindableProperty AngleProperty = BindableProperty.CreateAttached(
        "Angle",
        typeof(double),
        typeof(CircularLayout),
        Double.NaN,
        propertyChanged: OnChildPropertyChanged);

    public static readonly BindableProperty OrbitProperty = BindableProperty.CreateAttached(
        "Orbit",
        typeof(int),
        typeof(CircularLayout),
        1,
        propertyChanged: OnChildPropertyChanged);

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

    public bool RotateItems
    {
        get => (bool)GetValue(RotateItemsProperty);
        set => SetValue(RotateItemsProperty, value);
    }

    public double ItemAngle
    {
        get => (double)GetValue(ItemAngleProperty);
        set => SetValue(ItemAngleProperty, value);
    }

    public double OrbitSpacing
    {
        get => (double)GetValue(OrbitSpacingProperty);
        set => SetValue(OrbitSpacingProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Attached property
    //--------------------------------------------------------------------------------

    public static double GetAngle(BindableObject bindable) => (double)bindable.GetValue(AngleProperty);

    public static void SetAngle(BindableObject bindable, double value) => bindable.SetValue(AngleProperty, value);

    public static int GetOrbit(BindableObject bindable) => (int)bindable.GetValue(OrbitProperty);

    public static void SetOrbit(BindableObject bindable, int value) => bindable.SetValue(OrbitProperty, value);

    private static void OnChildPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Element { Parent: CircularLayout layout })
        {
            layout.InvalidateMeasure();
        }
    }

    //--------------------------------------------------------------------------------
    // Layout
    //--------------------------------------------------------------------------------

    private void OnRotateItemsChanged(bool rotate)
    {
        if (!rotate)
        {
            foreach (var child in this)
            {
                if (child is VisualElement element)
                {
                    element.Rotation = 0d;
                }
            }
        }

        InvalidateMeasure();
    }

    protected override ILayoutManager CreateLayoutManager() => new CircularLayoutManager(this);

    private readonly record struct Placement(IView View, int Orbit, double Angle, double Radius);

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
                var placements = ComputePlacements(children, layout.Radius);
                if (layout.FitToArc && (children.Count > 0))
                {
                    var extent = ComputeExtent(placements, maxChild);
                    return new Size(
                        extent.Width + layout.Padding.HorizontalThickness,
                        extent.Height + layout.Padding.VerticalThickness);
                }

                var outer = 0d;
                foreach (var placement in placements)
                {
                    outer = Math.Max(outer, placement.Radius);
                }

                var side = ((outer + (maxChild / 2d)) * 2d) + layout.Padding.HorizontalThickness;
                return new Size(side, side);
            }

            var width = Double.IsFinite(widthConstraint) ? widthConstraint : maxChild * UnboundedSizeRate;
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
                : AutoRadius(bounds, children, maxChild);
            var placements = ComputePlacements(children, radius);

            var cx = bounds.Center.X;
            var cy = bounds.Center.Y;
            if (layout.FitToArc && (layout.Radius > 0d))
            {
                var extent = ComputeExtent(placements, maxChild);
                cx -= extent.X + (extent.Width / 2d);
                cy -= extent.Y + (extent.Height / 2d);
            }

            var rotate = layout.RotateItems;
            foreach (var placement in placements)
            {
                var child = placement.View;
                var size = child.DesiredSize;
                var (px, py) = Offset(placement);
                var x = cx + px - (size.Width / 2d);
                var y = cy + py - (size.Height / 2d);
                if (rotate && (child is VisualElement element))
                {
                    element.Rotation = placement.Orbit > 0 ? placement.Angle + 90d + layout.ItemAngle : 0d;
                }

                child.Arrange(new Rect(x, y, size.Width, size.Height));
            }

            return bounds.Size;
        }

        private double AutoRadius(Rect bounds, List<IView> children, double maxChild)
        {
            var maxOrbit = 1;
            foreach (var child in children)
            {
                maxOrbit = Math.Max(maxOrbit, GetOrbit((BindableObject)child));
            }

            var outer = (Math.Min(bounds.Width, bounds.Height) / 2d) - (maxChild / 2d);
            return Math.Max(0d, outer - ((maxOrbit - 1) * layout.OrbitSpacing));
        }

        private List<Placement> ComputePlacements(List<IView> children, double baseRadius)
        {
            var counts = new Dictionary<int, int>();
            foreach (var child in children)
            {
                var orbit = Math.Max(0, GetOrbit((BindableObject)child));
                counts[orbit] = counts.TryGetValue(orbit, out var count) ? count + 1 : 1;
            }

            var sweep = layout.SweepAngle;
            var full = sweep >= 360d;
            var indices = new Dictionary<int, int>();
            var placements = new List<Placement>(children.Count);
            foreach (var child in children)
            {
                var bindable = (BindableObject)child;
                var orbit = Math.Max(0, GetOrbit(bindable));
                if (orbit == 0)
                {
                    placements.Add(new Placement(child, 0, Double.NaN, 0d));
                    continue;
                }

                var count = counts[orbit];
                var step = full || !layout.DistributeEvenly
                    ? sweep / count
                    : (count > 1 ? sweep / (count - 1) : 0d);
                var index = indices.GetValueOrDefault(orbit);
                indices[orbit] = index + 1;

                var angle = GetAngle(bindable);
                if (Double.IsNaN(angle))
                {
                    angle = layout.StartAngle + (step * index);
                }

                placements.Add(new Placement(child, orbit, angle, baseRadius + ((orbit - 1) * layout.OrbitSpacing)));
            }

            return placements;
        }

        private static (double X, double Y) Offset(Placement placement)
        {
            if (placement.Orbit == 0)
            {
                return (0d, 0d);
            }

            var rad = placement.Angle * Math.PI / 180d;
            return (placement.Radius * Math.Cos(rad), placement.Radius * Math.Sin(rad));
        }

        private static Rect ComputeExtent(List<Placement> placements, double maxChild)
        {
            var minX = Double.PositiveInfinity;
            var maxX = Double.NegativeInfinity;
            var minY = Double.PositiveInfinity;
            var maxY = Double.NegativeInfinity;
            var half = maxChild / 2d;
            foreach (var placement in placements)
            {
                var (x, y) = Offset(placement);
                minX = Math.Min(minX, x - half);
                maxX = Math.Max(maxX, x + half);
                minY = Math.Min(minY, y - half);
                maxY = Math.Max(maxY, y + half);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
