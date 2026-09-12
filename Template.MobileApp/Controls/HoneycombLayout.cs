namespace Template.MobileApp.Controls;

using Microsoft.Maui.Layouts;

// 子要素をハニカム (六角形の敷き詰め) 状に配置するカスタムレイアウト。
// 列を交互に半セル分ずらし、上にあるセルから順に埋める。3 列に 7 個置くと中央 1 + 周囲 6 の花形になる
public sealed class HoneycombLayout : Layout
{
    // 列数
    public static readonly BindableProperty ColumnsProperty = BindableProperty.Create(
        nameof(Columns),
        typeof(int),
        typeof(HoneycombLayout),
        3,
        propertyChanged: static (bindable, _, _) => ((HoneycombLayout)bindable).InvalidateMeasure());

    // セル間の余白
    public static readonly BindableProperty SpacingProperty = BindableProperty.Create(
        nameof(Spacing),
        typeof(double),
        typeof(HoneycombLayout),
        0d,
        propertyChanged: static (bindable, _, _) => ((HoneycombLayout)bindable).InvalidateMeasure());

    // 半セル下げる列 (true=偶数列 / false=奇数列)
    public static readonly BindableProperty StaggerEvenColumnsProperty = BindableProperty.Create(
        nameof(StaggerEvenColumns),
        typeof(bool),
        typeof(HoneycombLayout),
        true,
        propertyChanged: static (bindable, _, _) => ((HoneycombLayout)bindable).InvalidateMeasure());

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public bool StaggerEvenColumns
    {
        get => (bool)GetValue(StaggerEvenColumnsProperty);
        set => SetValue(StaggerEvenColumnsProperty, value);
    }

    protected override ILayoutManager CreateLayoutManager() => new HoneycombLayoutManager(this);

    private sealed class HoneycombLayoutManager : ILayoutManager
    {
        private readonly HoneycombLayout layout;

        public HoneycombLayoutManager(HoneycombLayout layout)
        {
            this.layout = layout;
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var count = 0;
            var cell = Size.Zero;
            foreach (var child in layout)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                var size = child.Measure(Double.PositiveInfinity, Double.PositiveInfinity);
                cell = new Size(Math.Max(cell.Width, size.Width), Math.Max(cell.Height, size.Height));
                count++;
            }

            var extent = ComputeExtent(count, cell);
            return new Size(
                extent.Width + layout.Padding.HorizontalThickness,
                extent.Height + layout.Padding.VerticalThickness);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var children = new List<IView>();
            var cell = Size.Zero;
            foreach (var child in layout)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                children.Add(child);
                cell = new Size(Math.Max(cell.Width, child.DesiredSize.Width), Math.Max(cell.Height, child.DesiredSize.Height));
            }

            var left = bounds.X + layout.Padding.Left;
            var top = bounds.Y + layout.Padding.Top;
            var slots = ComputeSlots(children.Count, cell);
            for (var i = 0; i < children.Count; i++)
            {
                children[i].Arrange(new Rect(left + slots[i].X, top + slots[i].Y, cell.Width, cell.Height));
            }

            return bounds.Size;
        }

        // 各セルの左上座標。ずらさない列とずらす列を半段ずつ交互に上から埋める
        private Point[] ComputeSlots(int count, Size cell)
        {
            var columns = Math.Max(1, layout.Columns);
            var stepX = cell.Width + layout.Spacing;
            var halfY = (cell.Height + layout.Spacing) / 2d;
            var slots = new Point[count];
            var index = 0;
            for (var half = 0; index < count; half++)
            {
                var staggered = (half % 2) == 1;
                for (var column = 0; (column < columns) && (index < count); column++)
                {
                    var even = (column % 2) == 0;
                    if ((even == layout.StaggerEvenColumns) == staggered)
                    {
                        slots[index++] = new Point(column * stepX, half * halfY);
                    }
                }
            }

            return slots;
        }

        // 全セルを含む外接サイズ
        private Size ComputeExtent(int count, Size cell)
        {
            if (count == 0)
            {
                return Size.Zero;
            }

            var slots = ComputeSlots(count, cell);
            var width = 0d;
            var height = 0d;
            foreach (var slot in slots)
            {
                width = Math.Max(width, slot.X + cell.Width);
                height = Math.Max(height, slot.Y + cell.Height);
            }

            return new Size(width, height);
        }
    }
}
