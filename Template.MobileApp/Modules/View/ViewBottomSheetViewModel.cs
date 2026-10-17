namespace Template.MobileApp.Modules.View;

using Fonts;

public sealed record BottomSheetAction(
    string Icon,
    string Text,
    Color Color,
    Color BackgroundColor);

public sealed partial class ViewBottomSheetViewModel : AppViewModelBase
{
    // Field

    private static readonly TimeSpan SwitchDelay = TimeSpan.FromMilliseconds(300);

    private static readonly Color NormalColor = Color.FromArgb("#546E7A");
    private static readonly Color NormalBackgroundColor = Color.FromArgb("#CFD8DC");
    private static readonly Color FavoriteColor = Color.FromArgb("#FF8F00");
    private static readonly Color FavoriteBackgroundColor = Color.FromArgb("#FFECB3");
    private static readonly Color ReportColor = Color.FromArgb("#F44336");
    private static readonly Color ReportBackgroundColor = Color.FromArgb("#FFCDD2");

    // Property

    [ObservableProperty]
    public partial bool IsSfSheetOpen { get; set; }

    [ObservableProperty]
    public partial bool IsCustomSheetOpen { get; set; }

    [ObservableProperty]
    public partial BottomSheetAction? Selected { get; set; }

    public IReadOnlyList<BottomSheetAction> Actions { get; } =
    [
        new(MaterialIcons.Share, "共有", NormalColor, NormalBackgroundColor),
        new(MaterialIcons.Link, "リンクをコピー", NormalColor, NormalBackgroundColor),
        new(MaterialIcons.Star, "お気に入りに追加", FavoriteColor, FavoriteBackgroundColor),
        new(MaterialIcons.Flag, "レポート", ReportColor, ReportBackgroundColor)
    ];

    // Command

    public IObserveCommand OpenSfSheetCommand { get; }

    public IObserveCommand OpenCustomSheetCommand { get; }

    public IObserveCommand CloseCommand { get; }

    public IObserveCommand SelectCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public ViewBottomSheetViewModel()
    {
        OpenSfSheetCommand = MakeDelegateCommand(() => IsSfSheetOpen = true);
        OpenCustomSheetCommand = MakeDelegateCommand(() => IsCustomSheetOpen = true);
        CloseCommand = MakeDelegateCommand(Close);
        SelectCommand = MakeDelegateCommand<BottomSheetAction>(x =>
        {
            Selected = x;
            Close();
        });
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2() => ToggleSheetAsync(true);

    protected override Task OnNotifyFunction3() => ToggleSheetAsync(false);

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private async Task ToggleSheetAsync(bool sf)
    {
        if (sf ? IsSfSheetOpen : IsCustomSheetOpen)
        {
            Close();
            return;
        }

        if (IsSfSheetOpen || IsCustomSheetOpen)
        {
            Close();
            await Task.Delay(SwitchDelay);
        }

        if (sf)
        {
            IsSfSheetOpen = true;
        }
        else
        {
            IsCustomSheetOpen = true;
        }
    }

    private void Close()
    {
        IsSfSheetOpen = false;
        IsCustomSheetOpen = false;
    }
}
