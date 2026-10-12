namespace Template.MobileApp.Modules.View;

using Fonts;

public sealed partial class DrawerMenuItem : ObservableObject
{
    public string Icon { get; }

    public string Text { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public DrawerMenuItem(string icon, string text)
    {
        Icon = icon;
        Text = text;
    }
}

// Syncfusion の SfNavigationDrawer と自作の SideDrawer (MauiComponents.Controls) を切り替えて比べる
public sealed partial class ViewDrawerViewModel : AppViewModelBase
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
    public partial DrawerMenuItem SelectedItem { get; set; }

    public IReadOnlyList<string> Modes { get; } = ["SfNavigationDrawer", "SideDrawer"];

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

    public ViewDrawerViewModel()
    {
        SelectedItem = MenuItems[0];
        SelectedItem.IsSelected = true;

        OpenCommand = MakeDelegateCommand(Toggle);
        SelectCommand = MakeDelegateCommand<DrawerMenuItem>(x =>
        {
            SelectedItem.IsSelected = false;
            SelectedItem = x;
            SelectedItem.IsSelected = true;
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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

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
