namespace Template.MobileApp.Models.Sample;

public partial class SwitchBotTemperature : ObservableObject
{
    [ObservableProperty]
    public partial string DeviceId { get; set; } = default!;

    [ObservableProperty]
    public partial DateTime Timestamp { get; set; }

    [ObservableProperty]
    public partial int Rssi { get; set; }

    [ObservableProperty]
    public partial double Temperature { get; set; }

    [ObservableProperty]
    public partial int Humidity { get; set; }

    [ObservableProperty]
    public partial int? Co2 { get; set; }
}
