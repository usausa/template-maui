namespace Template.MobileApp.Modules.View;

using Template.MobileApp.Controls;

public sealed partial class ViewCustomViewModel : AppViewModelBase
{
    private const int MaxAvatars = 8;

    private static readonly string[] AvatarPool =
    [
        "usa1_face.jpg",
        "usa2_face.jpg",
        "usa3_face.jpg",
        "usa4_face.jpg",
        "usa5_face.jpg",
        "usa6_face.jpg",
        "usa7_face.jpg",
        "usa8_face.jpg"
    ];

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

    // AvatarGroup デモ (Add / Remove で「+N」の変化を見る)
    public ObservableCollection<string> Avatars { get; } = [.. AvatarPool.Take(5)];

    // CompareSlider デモ (仕切り位置 0〜1)
    [ObservableProperty]
    public partial double ComparePosition { get; set; } = 0.5;

    public IObserveCommand AddAvatarCommand { get; }

    public IObserveCommand RemoveAvatarCommand { get; }

    public ViewCustomViewModel()
    {
        AddAvatarCommand = MakeDelegateCommand(() =>
        {
            if (Avatars.Count < MaxAvatars)
            {
                Avatars.Add(AvatarPool[Avatars.Count]);
            }
        });
        RemoveAvatarCommand = MakeDelegateCommand(() =>
        {
            if (Avatars.Count > 0)
            {
                Avatars.RemoveAt(Avatars.Count - 1);
            }
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
