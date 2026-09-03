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

    // Alert

    [ObservableProperty]
    public partial string AlertTitle { get; set; }

    [ObservableProperty]
    public partial string AlertMessage { get; set; }

    // Notification

    public ObservableCollection<SocialNotificationInfo> Notifications { get; } = [];

    // Status

    [ObservableProperty]
    public partial string StatusName { get; set; }

    [ObservableProperty]
    public partial string StatusForm { get; set; }

    [ObservableProperty]
    public partial int StatusHeal { get; set; }

    [ObservableProperty]
    public partial int StatusBuff { get; set; }

    [ObservableProperty]
    public partial int StatusDebuff { get; set; }

    // Information

    [ObservableProperty]
    public partial string InformationTitle { get; set; }

    public IReadOnlyList<SocialUnit> InformationUnits { get; }

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
        BackCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIMenu1));

        HasIconNotificationMail = true;
        HasIconNotificationInfo = true;

        Episode = "EP10 豊穣の雨作戦";

        PlayerExpPercent = 45;

        CountParts = 65536;
        CountGem = 30000;
        CountMoney = 1024000;

        AlertTitle = "BEAST ALERT";
        AlertMessage = "牛鬼級旅団出現";

        Notifications.Add(new SocialNotificationInfo { Category = "MELEE WEAPON", Name = "04式単分子長刀", Code = "SWOARD OF SLASSING", Percent = 87, Delay = 0 });
        Notifications.Add(new SocialNotificationInfo { Category = "GOLEM", Name = "大型機動兵器戦鎚IV型", Code = "WARHAMMER TYPE=4R", Percent = 65, Delay = 120 });
        Notifications.Add(new SocialNotificationInfo { Category = "VEHICLE", Name = "08式騎士輸送用装甲車両", Code = "SLEIPNIR", Percent = 23, Delay = 240 });

        StatusName = "甲種聖装 瑠璃";
        StatusForm = "A FORM";
        StatusHeal = 37200;
        StatusBuff = 48800;
        StatusDebuff = 23500;

        InformationTitle = "投入戦力 支援部隊";
        InformationUnits =
        [
            new SocialUnit { Name = "辺境伯直属戦術機甲大隊", Code = "WOLF GRP", Force = "MF-4000 x36" },
            new SocialUnit { Name = "第二騎士団聖女計画特務中隊", Code = "HOUND SQD", Force = "TYPE-19E x10 + JXD-20" },
            new SocialUnit { Name = "第三騎士団強襲突撃部隊", Code = "VIPPERS", Force = "TYPE-19 BLOOD x8" }
        ];

        HasMenuNotificationOperation = true;
        HasMenuNotificationWeaponStorage = true;
        HasMenuNotificationDevelopment = true;
    }
}
