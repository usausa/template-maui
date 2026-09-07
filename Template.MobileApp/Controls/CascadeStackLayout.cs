namespace Template.MobileApp.Controls;

using Microsoft.Maui.Layouts;

// 子要素を左上から右下へ重ねて並べるカスタムレイアウト (MDI ウィンドウ風)
public sealed class CascadeStackLayout : Layout
{
    // 子要素ごとにずらす量
    public static readonly BindableProperty OffsetProperty = BindableProperty.Create(
        nameof(Offset),
        typeof(double),
        typeof(CascadeStackLayout),
        20d,
        propertyChanged: static (bindable, _, _) => ((CascadeStackLayout)bindable).InvalidateMeasure());

    public double Offset
    {
        get => (double)GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    protected override ILayoutManager CreateLayoutManager() => new CascadeLayoutManager(this);

    private sealed class CascadeLayoutManager : ILayoutManager
    {
        private readonly CascadeStackLayout layout;

        public CascadeLayoutManager(CascadeStackLayout layout)
        {
            this.layout = layout;
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var padding = layout.Padding;
            var maxWidth = 0d;
            var maxHeight = 0d;
            var count = 0;
            foreach (var child in layout)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                var size = child.Measure(Double.PositiveInfinity, Double.PositiveInfinity);
                maxWidth = Math.Max(maxWidth, size.Width);
                maxHeight = Math.Max(maxHeight, size.Height);
                count++;
            }

            var cascade = count > 0 ? (count - 1) * layout.Offset : 0d;
            return new Size(
                maxWidth + cascade + padding.HorizontalThickness,
                maxHeight + cascade + padding.VerticalThickness);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var padding = layout.Padding;
            var index = 0;
            foreach (var child in layout)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                var size = child.DesiredSize;
                var x = bounds.X + padding.Left + (index * layout.Offset);
                var y = bounds.Y + padding.Top + (index * layout.Offset);
                child.Arrange(new Rect(x, y, size.Width, size.Height));
                index++;
            }

            return bounds.Size;
        }
    }
}
