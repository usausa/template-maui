namespace Template.MobileApp.Modules.View;

using Template.MobileApp.Controls;

public sealed partial class ViewCustomViewModel : AppViewModelBase
{
    public IReadOnlyList<TreeNode> Nodes { get; } =
    [
        new TreeNode(
            "Template.MobileApp",
            new TreeNode(
                "Controls",
                new TreeNode("MarqueeLabel.cs"),
                new TreeNode("TreeView.cs"),
                new TreeNode("ColorPicker.cs")),
            new TreeNode(
                "Modules",
                new TreeNode(
                    "Basic",
                    new TreeNode("BasicMenuView.xaml"),
                    new TreeNode("BasicSettingView.xaml")),
                new TreeNode(
                    "View",
                    new TreeNode("ViewLayoutView.xaml"),
                    new TreeNode("ViewCustomView.xaml"))),
            new TreeNode(
                "Resources",
                new TreeNode("Styles"),
                new TreeNode("Fonts"))),
        new TreeNode(
            "Document",
            new TreeNode("Development.md"),
            new TreeNode("UI_Development_Log.md"))
    ];

    [ObservableProperty]
    public partial TreeNode? SelectedNode { get; set; }

    [ObservableProperty]
    public partial Color PickedColor { get; set; } = Color.FromArgb("#2196F3");

    [ObservableProperty]
    public partial TimeSpan Duration { get; set; } = new(1, 30, 0);

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
