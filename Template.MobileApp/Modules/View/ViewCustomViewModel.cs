namespace Template.MobileApp.Modules.View;

using Template.MobileApp.Controls;

public sealed partial class ViewCustomViewModel : AppViewModelBase
{
    public IReadOnlyList<TreeNode> Nodes { get; } =
    [
        new(
            "Template.MobileApp",
            new(
                "Controls",
                new("MarqueeLabel.cs"),
                new("TreeView.cs"),
                new("ColorPicker.cs")),
            new(
                "Modules",
                new(
                    "Basic",
                    new("BasicMenuView.xaml"),
                    new("BasicSettingView.xaml")),
                new(
                    "View",
                    new("ViewLayoutView.xaml"),
                    new("ViewCustomView.xaml"))),
            new(
                "Resources",
                new("Styles"),
                new("Fonts"))),
        new(
            "Document",
            new("Development.md"),
            new("UI_Development_Log.md"))
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
