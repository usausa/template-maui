namespace Template.MobileApp.Modules.Control;

using Fonts;

public sealed record DrawerMenuItem(string Icon, string Text);

// Syncfusion の SfNavigationDrawer と自作の SideDrawer (Controls/SideDrawer.cs) を切り替えて比べる
public sealed partial class ControlDrawerViewModel : AppViewModelBase
{
    // 0 = Sf / 1 = 自作
    [ObservableProperty]
    public partial int ModeIndex { get; set; }

    [ObservableProperty]
    public partial bool IsSfMode { get; set; } = true;

    [ObservableProperty]
    public partial bool IsCustomMode { get; set; }

    [ObservableProperty]
    public partial bool IsSfDrawerOpen { get; set; }

    [ObservableProperty]
    public partial bool IsCustomDrawerOpen { get; set; }

    [ObservableProperty]
    public partial string Selected { get; set; } = "ホーム";

    public IReadOnlyList<string> Modes { get; } = ["SfNavigationDrawer", "自作 SideDrawer"];

    public IReadOnlyList<DrawerMenuItem> MenuItems { get; } =
    [
        new(MaterialIcons.Home, "ホーム"),
        new(MaterialIcons.Inbox, "受信トレイ"),
        new(MaterialIcons.Star, "お気に入り"),
        new(MaterialIcons.History, "履歴"),
        new(MaterialIcons.Settings, "設定")
    ];

    public IObserveCommand OpenCommand { get; }

    public IObserveCommand SelectCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public ControlDrawerViewModel()
    {
        OpenCommand = MakeDelegateCommand(Toggle);
        SelectCommand = MakeDelegateCommand<DrawerMenuItem>(x =>
        {
            Selected = x.Text;
            IsSfDrawerOpen = false;
            IsCustomDrawerOpen = false;
        });

        Disposables.Add(this.AsObservable(nameof(ModeIndex)).Subscribe(_ =>
        {
            IsSfMode = ModeIndex == 0;
            IsCustomMode = ModeIndex == 1;
            IsSfDrawerOpen = false;
            IsCustomDrawerOpen = false;
        }));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ControlMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Toggle();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Toggle()
    {
        if (IsSfMode)
        {
            IsSfDrawerOpen = !IsSfDrawerOpen;
        }
        else
        {
            IsCustomDrawerOpen = !IsCustomDrawerOpen;
        }
    }
}
