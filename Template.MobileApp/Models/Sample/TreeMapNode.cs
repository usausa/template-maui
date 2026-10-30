namespace Template.MobileApp.Models.Sample;

public sealed class TreeMapNode<T>
{
    public T Value { get; } = default!;

    public double Area { get; }

    public TreeMapNode<T> Left { get; } = default!;

    public TreeMapNode<T> Right { get; } = default!;

    public bool IsLeaf { get; }

    private TreeMapNode(T value, double area)
    {
        Value = value;
        Area = area;
        IsLeaf = true;
    }

    private TreeMapNode(TreeMapNode<T> left, TreeMapNode<T> right)
    {
        Left = left;
        Right = right;
        Area = left.Area + right.Area;
        IsLeaf = false;
    }

#pragma warning disable CA1000
    public static TreeMapNode<T> Build(IEnumerable<T> values, Func<T, double> areaSelector)
    {
        return BuildRecursive(values
            .Select(value => new TreeMapNode<T>(value, areaSelector(value)))
            .OrderByDescending(node => node.Area)
            .ToArray());
    }
#pragma warning restore CA1000

    private static TreeMapNode<T> BuildRecursive(ReadOnlySpan<TreeMapNode<T>> nodes)
    {
        if (nodes.Length == 1)
        {
            return nodes[0];
        }

        var total = 0d;
        foreach (var node in nodes)
        {
            total += node.Area;
        }

        var split = 0;
        var sum = 0d;
        for (var i = 0; i < nodes.Length; i++)
        {
            sum += nodes[i].Area;
            if (sum >= total / 2)
            {
                split = i + 1;
                break;
            }
        }

        split = Math.Clamp(split, 1, nodes.Length - 1);

        var left = BuildRecursive(nodes[..split]);
        var right = BuildRecursive(nodes[split..]);

        return new TreeMapNode<T>(left, right);
    }
}
