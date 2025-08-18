namespace Template.MobileApp.Modules.UI;

public sealed partial class UISocialViewModel : AppViewModelBase
{
    // Notification

    [ObservableProperty]
    public partial bool HasIconNotificationMail { get; set; }

    [ObservableProperty]
    public partial bool HasIconNotificationInfo { get; set; }

    // Episode

    [ObservableProperty]
    public partial string Episode { get; set; }

    // Player

    [ObservableProperty]
    public partial double PlayerExpPercent { get; set; }

    // Count

    [ObservableProperty]
    public partial int CountParts { get; set; }

    [ObservableProperty]
    public partial int CountGem { get; set; }

    [ObservableProperty]
    public partial int CountMoney { get; set; }

    // Status

    [ObservableProperty]
    public partial int StatusHeal { get; set; }

    [ObservableProperty]
    public partial int StatusBuff { get; set; }

    [ObservableProperty]
    public partial int StatusDebuff { get; set; }

    // Menu

    [ObservableProperty]
    public partial bool HasMenuNotificationOperation { get; set; }

    [ObservableProperty]
    public partial bool HasMenuNotificationFormation { get; set; }

    [ObservableProperty]
    public partial bool HasMenuNotificationArt { get; set; }

    [ObservableProperty]
    public partial bool HasMenuNotificationHangar { get; set; }

    [ObservableProperty]
    public partial bool HasMenuNotificationWeaponStorage { get; set; }

    [ObservableProperty]
    public partial bool HasMenuNotificationDevelopment { get; set; }

    [ObservableProperty]
    public partial bool HasMenuNotificationHeadquarter { get; set; }

    [ObservableProperty]
    public partial bool HasMenuNotificationLive { get; set; }

    public IObserveCommand BackCommand { get; }

    public UISocialViewModel()
    {
        BackCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIMenu));

        HasIconNotificationMail = true;
        HasIconNotificationInfo = true;

        Episode = "EP10 豊穣の雨作戦";

        PlayerExpPercent = 45;

        CountParts = 65536;
        CountGem = 30000;
        CountMoney = 1024000;

        StatusHeal = 37200;
        StatusBuff = 48800;
        StatusDebuff = 23500;

        HasMenuNotificationOperation = true;
        HasMenuNotificationWeaponStorage = true;
        HasMenuNotificationDevelopment = true;
    }
}
