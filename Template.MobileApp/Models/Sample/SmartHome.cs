namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum HomeDeviceKind
{
    Light,
    Blinds,
    Lock,
    Lamp,
    Fan,
    AirConditioner
}

public enum HomeDeviceStatus
{
    Online,
    Active,
    Issue
}

public enum HomeRoomKind
{
    LivingRoom,
    Bedroom,
    Kitchen
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class HomeDevice : ObservableObject
{
    private double lastLevel = 100;

    public string Name { get; set; } = default!;

    public HomeDeviceKind Kind { get; set; }

    public HomeDeviceStatus Status { get; set; }

    [ObservableProperty]
    public partial bool IsOn { get; set; }

    // 明るさと開き具合は %、Fan は風量の段、AirConditioner は設定の温度
    [ObservableProperty]
    public partial double Level { get; set; }

    // 明るさの変わる機器は消すと 0% にして、点けると消す前の明るさに戻す
    public void Toggle()
    {
        IsOn = !IsOn;
        if (Kind is HomeDeviceKind.Light or HomeDeviceKind.Lamp)
        {
            if (IsOn)
            {
                Level = lastLevel;
            }
            else
            {
                lastLevel = Level > 0 ? Level : lastLevel;
                Level = 0;
            }
        }
    }
}

public sealed class HomeBattery
{
    public string Name { get; set; } = default!;

    // 0〜1
    public double Level { get; set; }

    public HomeDeviceStatus Status { get; set; }
}

public sealed partial class HomeRoom : ObservableObject
{
    public HomeRoomKind Kind { get; set; }

    public string Name { get; set; } = default!;

    public string Background { get; set; } = default!;

    public double Temperature { get; set; }

    public string Scene { get; set; } = default!;

    [ObservableProperty]
    public partial double SetTemperature { get; set; }

    [ObservableProperty]
    public partial bool IsThermostatActive { get; set; }

    public IReadOnlyList<HomeDevice> Devices { get; set; } = [];

    public IReadOnlyList<HomeBattery> Batteries { get; set; } = [];
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed partial class SmartHome : ObservableObject
{
    public const double MinTemperature = 16d;

    public const double MaxTemperature = 30d;

    public const double TemperatureStep = 0.5d;

    public IReadOnlyList<HomeRoom> Rooms { get; }

    [ObservableProperty]
    public partial HomeRoom Room { get; set; }

    public SmartHome(IReadOnlyList<HomeRoom> rooms)
    {
        Rooms = rooms;
        Room = rooms[0];
    }

    public void Select(HomeRoom room) => Room = room;

    public void ChangeTemperature(double delta) =>
        Room.SetTemperature = Math.Clamp(Room.SetTemperature + delta, MinTemperature, MaxTemperature);
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// 部屋と機器の見本 (リビング・寝室・キッチン)
public static class HomeSample
{
    public static IReadOnlyList<HomeRoom> LoadRooms() =>
    [
        new()
        {
            Kind = HomeRoomKind.LivingRoom,
            Name = "リビング",
            Background = "gallery05.jpg",
            Temperature = 21,
            Scene = "夕暮れ",
            SetTemperature = 22.5,
            IsThermostatActive = true,
            Devices =
            [
                new HomeDevice { Name = "メイン照明", Kind = HomeDeviceKind.Light, IsOn = true, Level = 75 },
                new HomeDevice { Name = "ブラインド", Kind = HomeDeviceKind.Blinds, IsOn = true, Level = 60 },
                new HomeDevice { Name = "玄関の鍵", Kind = HomeDeviceKind.Lock, IsOn = true },
                new HomeDevice { Name = "フロアランプ", Kind = HomeDeviceKind.Lamp, Level = 0 },
                new HomeDevice { Name = "天井ファン", Kind = HomeDeviceKind.Fan, IsOn = true, Level = 2 },
                new HomeDevice { Name = "エアコン", Kind = HomeDeviceKind.AirConditioner, IsOn = true, Level = 22.5 }
            ],
            Batteries =
            [
                new HomeBattery { Name = "ドアロック", Level = 0.92 },
                new HomeBattery { Name = "センサーハブ", Level = 0.78 },
                new HomeBattery { Name = "防犯カメラ", Level = 0.54, Status = HomeDeviceStatus.Active }
            ]
        },
        new()
        {
            Kind = HomeRoomKind.Bedroom,
            Name = "寝室",
            Background = "gallery06.jpg",
            Temperature = 20,
            Scene = "おやすみ",
            SetTemperature = 20.5,
            IsThermostatActive = true,
            Devices =
            [
                new HomeDevice { Name = "天井の照明", Kind = HomeDeviceKind.Light, Level = 0 },
                new HomeDevice { Name = "ブラインド", Kind = HomeDeviceKind.Blinds, Level = 0 },
                new HomeDevice { Name = "窓の鍵", Kind = HomeDeviceKind.Lock, IsOn = true },
                new HomeDevice { Name = "枕元ランプ", Kind = HomeDeviceKind.Lamp, IsOn = true, Level = 30 },
                new HomeDevice { Name = "空気清浄機", Kind = HomeDeviceKind.Fan, IsOn = true, Level = 1 },
                new HomeDevice { Name = "エアコン", Kind = HomeDeviceKind.AirConditioner, IsOn = true, Level = 20.5 }
            ],
            Batteries =
            [
                new HomeBattery { Name = "ドアセンサー", Level = 0.66 },
                new HomeBattery { Name = "リモコン", Level = 0.35, Status = HomeDeviceStatus.Active },
                new HomeBattery { Name = "火災報知器", Level = 0.18, Status = HomeDeviceStatus.Issue }
            ]
        },
        new()
        {
            Kind = HomeRoomKind.Kitchen,
            Name = "キッチン",
            Background = "gallery02.jpg",
            Temperature = 23,
            Scene = "朝",
            SetTemperature = 23,
            Devices =
            [
                new HomeDevice { Name = "スポット照明", Kind = HomeDeviceKind.Light, IsOn = true, Level = 90 },
                new HomeDevice { Name = "ブラインド", Kind = HomeDeviceKind.Blinds, IsOn = true, Level = 100 },
                new HomeDevice { Name = "勝手口の鍵", Kind = HomeDeviceKind.Lock, Status = HomeDeviceStatus.Issue },
                new HomeDevice { Name = "手元ランプ", Kind = HomeDeviceKind.Lamp, Level = 0 },
                new HomeDevice { Name = "換気扇", Kind = HomeDeviceKind.Fan, IsOn = true, Level = 3, Status = HomeDeviceStatus.Active },
                new HomeDevice { Name = "エアコン", Kind = HomeDeviceKind.AirConditioner, Level = 24 }
            ],
            Batteries =
            [
                new HomeBattery { Name = "防犯カメラ", Level = 0.54, Status = HomeDeviceStatus.Active },
                new HomeBattery { Name = "漏水センサー", Level = 0.81 },
                new HomeBattery { Name = "ドアセンサー", Level = 0.27, Status = HomeDeviceStatus.Issue }
            ]
        }
    ];
}
