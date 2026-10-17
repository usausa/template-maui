namespace Template.MobileApp.Modules.View;

using MauiComponents.Controls;

public sealed partial class ViewCustomViewModel : AppViewModelBase
{
    private const int MaxAvatars = 8;

    // Field

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

    // Property

    public IReadOnlyList<TreeNode> Nodes { get; } =
    [
        new(
            "Template.MobileApp",
            new(
                "Controls",
                new("ColorPicker.cs"),
                new("MarqueeLabel.cs"),
                new("TreeView.cs")),
            new(
                "Modules",
                new(
                    "Basic",
                    new("BasicMenuView.xaml"),
                    new("BasicSettingView.xaml")),
                new(
                    "View",
                    new("ViewCustomView.xaml"),
                    new("ViewLayoutView.xaml"))),
            new(
                "Resources",
                new(
                    "Fonts",
                    new("MaterialIcons-Regular.ttf"),
                    new("OpenSans-Regular.ttf")),
                new(
                    "Styles",
                    new("Colors.xaml"),
                    new("Styles.xaml"))))
    ];

    [ObservableProperty]
    public partial TreeNode? SelectedNode { get; set; }

    [ObservableProperty]
    public partial Color PickedColor { get; set; } = Color.FromArgb("#2196F3");

    [ObservableProperty]
    public partial TimeSpan Duration { get; set; } = new(1, 30, 0);

    public ObservableCollection<string> Avatars { get; } = [.. AvatarPool.Take(5)];

    public IObserveCommand AddAvatarCommand { get; }

    public IObserveCommand RemoveAvatarCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
