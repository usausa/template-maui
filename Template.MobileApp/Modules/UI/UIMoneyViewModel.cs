namespace Template.MobileApp.Modules.UI;

public enum MoneyPage
{
    Home,
    Search,
    Notifications,
    Account
}

public sealed partial class UIMoneyViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial MoneyPage Selected { get; set; }

    [ObservableProperty]
    public partial int NotificationCount { get; set; }

    [ObservableProperty]
    public partial bool HasAccountAlert { get; set; }

    [ObservableProperty]
    public partial double Balance { get; set; }

    public ICommand PageCommand { get; }

    public UIMoneyViewModel()
    {
        Selected = MoneyPage.Home;

        PageCommand = MakeDelegateCommand<MoneyPage>(page => Selected = page);

        NotificationCount = 128;
        HasAccountAlert = true;
        Balance = 20000;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
