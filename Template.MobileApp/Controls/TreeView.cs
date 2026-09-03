namespace Template.MobileApp.Controls;

public sealed class TreeNode
{
    public string Text { get; }

    public IReadOnlyList<TreeNode> Children { get; }

    public bool HasChildren => Children.Count > 0;

    public bool IsExpanded { get; set; }

    public TreeNode(string text, params TreeNode[] children)
    {
        Text = text;
        Children = children;
    }
}

// 階層データを展開/折りたたみ付きで表示する簡易 TreeView。
// 表示中のノードをフラット化して並べ直す方式 (サンプル規模向け)
public sealed class TreeView : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IReadOnlyList<TreeNode>),
        typeof(TreeView),
        propertyChanged: static (bindable, _, _) => ((TreeView)bindable).Rebuild());

    public static readonly BindableProperty SelectedNodeProperty = BindableProperty.Create(
        nameof(SelectedNode),
        typeof(TreeNode),
        typeof(TreeView),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: static (bindable, _, _) => ((TreeView)bindable).Rebuild());

    public static readonly BindableProperty IndentSizeProperty = BindableProperty.Create(
        nameof(IndentSize),
        typeof(double),
        typeof(TreeView),
        20d);

    public IReadOnlyList<TreeNode>? ItemsSource
    {
        get => (IReadOnlyList<TreeNode>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public TreeNode? SelectedNode
    {
        get => (TreeNode?)GetValue(SelectedNodeProperty);
        set => SetValue(SelectedNodeProperty, value);
    }

    public double IndentSize
    {
        get => (double)GetValue(IndentSizeProperty);
        set => SetValue(IndentSizeProperty, value);
    }

    private static readonly Color ChevronColor = Color.FromArgb("#90A4AE");
    private static readonly Color TextColor = Color.FromArgb("#37474F");
    private static readonly Color SelectedBackground = Color.FromArgb("#BBDEFB");

    private readonly VerticalStackLayout container;

    public TreeView()
    {
        container = new VerticalStackLayout { Spacing = 2 };
        Content = container;
    }

    private void Rebuild()
    {
        container.Children.Clear();

        if (ItemsSource is { } roots)
        {
            AddNodes(roots, 0);
        }
    }

    private void AddNodes(IReadOnlyList<TreeNode> nodes, int depth)
    {
        foreach (var node in nodes)
        {
            container.Children.Add(BuildRow(node, depth));
            if (node.HasChildren && node.IsExpanded)
            {
                AddNodes(node.Children, depth + 1);
            }
        }
    }

    private Grid BuildRow(TreeNode node, int depth)
    {
        var row = new Grid
        {
            Padding = new Thickness((depth * IndentSize) + 4, 6, 4, 6),
            ColumnSpacing = 4,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            BackgroundColor = ReferenceEquals(node, SelectedNode) ? SelectedBackground : Colors.Transparent
        };

        var chevron = new Label
        {
            Text = node.HasChildren ? (node.IsExpanded ? "▾" : "▸") : "・",
            TextColor = ChevronColor,
            FontSize = 14,
            WidthRequest = 18,
            VerticalTextAlignment = TextAlignment.Center
        };
        row.Add(chevron, 0);

        var text = new Label
        {
            Text = node.Text,
            TextColor = TextColor,
            FontSize = 14,
            VerticalTextAlignment = TextAlignment.Center
        };
        row.Add(text, 1);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (node.HasChildren)
            {
                node.IsExpanded = !node.IsExpanded;
            }

            SelectedNode = node;
            Rebuild();
        };
        row.GestureRecognizers.Add(tap);

        return row;
    }
}
