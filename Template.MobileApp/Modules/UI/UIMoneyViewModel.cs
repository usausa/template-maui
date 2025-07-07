namespace Template.MobileApp.Modules.UI;

public sealed partial class UIMoneyViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial MoneyPage Selected { get; set; }

    public ICommand PageCommand { get; }

    public UIMoneyViewModel()
    {
        Selected = MoneyPage.Home;

        PageCommand = MakeDelegateCommand<MoneyPage>(page => Selected = page);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
