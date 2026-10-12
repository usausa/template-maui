namespace Template.MobileApp.Modules.UI;

public sealed partial class UIMailDetailViewModel : AppViewModelBase
{
    [Scope]
    [ObservableProperty]
    public partial UIMailContext Context { get; set; } = default!;

    public IObserveCommand StarCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIMailDetailViewModel()
    {
        StarCommand = MakeDelegateCommand(ToggleStar);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.PopAsync();

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    // 削除して一覧へ
    protected override Task OnNotifyFunction4()
    {
        Context.Delete(Context.Selected);
        return Navigator.PopAsync();
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ToggleStar() => Context.ToggleStar(Context.Selected);
}
