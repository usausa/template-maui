namespace Template.MobileApp.Layouts;

using Microsoft.Maui.Layouts;

// ILayoutManagerFactory のデモ。
// レイアウト型ごとにレイアウトマネージャを DI で差し替えられるフックで、
// ここでは CascadeStackLayout (VerticalStackLayout 派生のマーカー型) のときだけ
// カスケード配置のマネージャを返し、他の型は null (= 既定のマネージャ) とする
public sealed class CascadeStackLayout : VerticalStackLayout;

public sealed class AppLayoutManagerFactory : ILayoutManagerFactory
{
    public ILayoutManager? CreateLayoutManager(Layout layout) =>
        layout is CascadeStackLayout cascade ? new CascadeLayoutManager(cascade) : null;
}

// 子要素を左上から右下へ重ねて並べる (MDI ウィンドウ風)
public sealed class CascadeLayoutManager : ILayoutManager
{
    private const double Offset = 20d;

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

        var cascade = count > 0 ? (count - 1) * Offset : 0d;
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
            var x = bounds.X + padding.Left + (index * Offset);
            var y = bounds.Y + padding.Top + (index * Offset);
            child.Arrange(new Rect(x, y, size.Width, size.Height));
            index++;
        }

        return bounds.Size;
    }
}
