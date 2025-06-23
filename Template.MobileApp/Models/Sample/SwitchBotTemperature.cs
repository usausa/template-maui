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

public static class SwitchBotTemperatureExtensions
{
    public static void CopyTo(this SwitchBotTemperature source, SwitchBotTemperature target)
    {
        target.DeviceId = source.DeviceId;
        target.Timestamp = source.Timestamp;
        target.Rssi = source.Rssi;
        target.Temperature = source.Temperature;
        target.Humidity = source.Humidity;
        target.Co2 = source.Co2;
    }
}
