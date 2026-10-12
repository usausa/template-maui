namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

// 見出しのアイコンとメニューの新着の印
[Flags]
public enum SocialBadges
{
    None = 0,
    Mail = 1 << 0,
    Info = 1 << 1,
    Operation = 1 << 2,
    Formation = 1 << 3,
    Art = 1 << 4,
    Hangar = 1 << 5,
    WeaponStorage = 1 << 6,
    Development = 1 << 7,
    Headquarter = 1 << 8,
    Live = 1 << 9
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class SocialUnit
{
    public required string Name { get; init; }

    public required string Code { get; init; }

    public required string Force { get; init; }
}

public sealed class SocialNotificationInfo
{
    public required string Category { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public required double Percent { get; init; }

    // 表示時の時間差スライドイン用ディレイ(ms)
    public int Delay { get; init; }
}

// プレイヤーの状況 (エピソード・経験値・所持数)
public sealed record SocialPlayer(string Episode, double ExpPercent, int Parts, int Gem, int Money);

// 警報
public sealed record SocialAlert(string Title, string Message);

// 状態 (回復・強化・弱体)
public sealed record SocialStatus(string Name, string Form, int Heal, int Buff, int Debuff);

// 投入戦力
public sealed record SocialInformation(string Title, IReadOnlyList<SocialUnit> Units);

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// ソーシャルゲームのホームの見本
public static class SocialSample
{
    public static SocialBadges LoadBadges() =>
        SocialBadges.Mail | SocialBadges.Info | SocialBadges.Operation | SocialBadges.WeaponStorage | SocialBadges.Development;

    public static SocialPlayer LoadPlayer() =>
        new(Episode: "EP10 豊穣の雨作戦", ExpPercent: 45, Parts: 65536, Gem: 30000, Money: 1024000);

    public static SocialAlert LoadAlert() => new(Title: "BEAST ALERT", Message: "牛鬼級旅団出現");

    public static IReadOnlyList<SocialNotificationInfo> LoadNotifications() =>
    [
        new() { Category = "MELEE WEAPON", Name = "04式単分子長刀", Code = "SWOARD OF SLASSING", Percent = 87, Delay = 0 },
        new() { Category = "GOLEM", Name = "大型機動兵器戦鎚IV型", Code = "WARHAMMER TYPE=4R", Percent = 65, Delay = 120 },
        new() { Category = "VEHICLE", Name = "08式騎士輸送用装甲車両", Code = "SLEIPNIR", Percent = 23, Delay = 240 }
    ];

    public static SocialStatus LoadStatus() =>
        new(Name: "甲種聖装 瑠璃", Form: "A FORM", Heal: 37200, Buff: 48800, Debuff: 23500);

    public static SocialInformation LoadInformation() =>
        new(
            "投入戦力 支援部隊",
            [
                new() { Name = "辺境伯直属戦術機甲大隊", Code = "WOLF GRP", Force = "MF-4000 x36" },
                new() { Name = "第二騎士団聖女計画特務中隊", Code = "HOUND SQD", Force = "TYPE-19E x10 + JXD-20" },
                new() { Name = "第三騎士団強襲突撃部隊", Code = "VIPPERS", Force = "TYPE-19 BLOOD x8" }
            ]);
}
