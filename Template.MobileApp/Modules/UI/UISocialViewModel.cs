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

    public ObservableCollection<SocialNotificationInfo> Notifications { get; }

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

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UISocialViewModel()
    {
        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);

        var badges = SocialSample.LoadBadges();
        HasIconNotificationMail = badges.HasFlag(SocialBadges.Mail);
        HasIconNotificationInfo = badges.HasFlag(SocialBadges.Info);

        var player = SocialSample.LoadPlayer();
        Episode = player.Episode;
        PlayerExpPercent = player.ExpPercent;
        CountParts = player.Parts;
        CountGem = player.Gem;
        CountMoney = player.Money;

        var alert = SocialSample.LoadAlert();
        AlertTitle = alert.Title;
        AlertMessage = alert.Message;

        Notifications = [.. SocialSample.LoadNotifications()];

        var status = SocialSample.LoadStatus();
        StatusName = status.Name;
        StatusForm = status.Form;
        StatusHeal = status.Heal;
        StatusBuff = status.Buff;
        StatusDebuff = status.Debuff;

        var information = SocialSample.LoadInformation();
        InformationTitle = information.Title;
        InformationUnits = information.Units;

        HasMenuNotificationOperation = badges.HasFlag(SocialBadges.Operation);
        HasMenuNotificationFormation = badges.HasFlag(SocialBadges.Formation);
        HasMenuNotificationArt = badges.HasFlag(SocialBadges.Art);
        HasMenuNotificationHangar = badges.HasFlag(SocialBadges.Hangar);
        HasMenuNotificationWeaponStorage = badges.HasFlag(SocialBadges.WeaponStorage);
        HasMenuNotificationDevelopment = badges.HasFlag(SocialBadges.Development);
        HasMenuNotificationHeadquarter = badges.HasFlag(SocialBadges.Headquarter);
        HasMenuNotificationLive = badges.HasFlag(SocialBadges.Live);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);
}
