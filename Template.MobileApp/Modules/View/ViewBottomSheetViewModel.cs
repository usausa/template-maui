namespace Template.MobileApp.Modules.View;

public sealed partial class ViewBottomSheetViewModel : AppViewModelBase
{
    // 閉じるアニメーションを待ってから次のシートを開く
    private static readonly TimeSpan SwitchDelay = TimeSpan.FromMilliseconds(300);

    [ObservableProperty]
    public partial bool IsSfSheetOpen { get; set; }

    [ObservableProperty]
    public partial bool IsCustomSheetOpen { get; set; }

    [ObservableProperty]
    public partial string? Selected { get; set; }

    public IReadOnlyList<string> Actions { get; } = ["共有", "リンクをコピー", "お気に入りに追加", "レポート"];

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
        SelectCommand = MakeDelegateCommand<string>(x =>
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
