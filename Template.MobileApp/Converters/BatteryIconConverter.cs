namespace Template.MobileApp.Converters;

using BatteryState = Microsoft.Maui.Devices.BatteryState;

public sealed class BatteryIconConverter : IMultiValueConverter
{
    public string? Unknown { get; set; }

    public string? Alert { get; set; }

    public string? Charging { get; set; }

    public string? Full { get; set; }

    public string? Level6 { get; set; }

    public string? Level5 { get; set; }

    public string? Level4 { get; set; }

    public string? Level3 { get; set; }

    public string? Level2 { get; set; }

    public string? Level1 { get; set; }

    public string? Level0 { get; set; }

    public object? Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if ((values is null) || (values.Length != 3))
        {
            return Unknown;
        }

        if (values[2] is not BatteryPowerSource source)
        {
            return Unknown;
        }

        if ((source == BatteryPowerSource.AC) || (source == BatteryPowerSource.Usb) || (source == BatteryPowerSource.Wireless))
        {
            return Charging;
        }

        if (values[1] is not BatteryState state)
        {
            return Unknown;
        }

        if (state == BatteryState.Charging)
        {
            return Charging;
        }
        if ((state == BatteryState.Unknown) || (state == BatteryState.NotPresent))
        {
            return Alert;
        }

        if (values[0] is not double chargeLevel)
        {
            return Unknown;
        }

        if (chargeLevel >= 0.875)
        {
            return Full;
        }
        if (chargeLevel >= 0.75)
        {
            return Level6;
        }
        if (chargeLevel >= 0.625)
        {
            return Level5;
        }
        if (chargeLevel >= 0.5)
        {
            return Level4;
        }
        if (chargeLevel >= 0.375)
        {
            return Level3;
        }
        if (chargeLevel >= 0.25)
        {
            return Level2;
        }
        if (chargeLevel >= 0.125)
        {
            return Level1;
        }

        return Level0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
