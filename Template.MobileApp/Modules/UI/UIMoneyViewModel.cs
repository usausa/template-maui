namespace Template.MobileApp.Modules.UI;

public sealed partial class UIMoneyViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial MoneyPage Selected { get; set; }

    [ObservableProperty]
    public partial int NotificationCount { get; set; }

    public ICommand PageCommand { get; }

    public UIMoneyViewModel()
    {
        Selected = MoneyPage.Home;

        PageCommand = MakeDelegateCommand<MoneyPage>(page => Selected = page);

        NotificationCount = 99;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
