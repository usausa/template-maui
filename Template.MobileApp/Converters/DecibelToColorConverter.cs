namespace Template.MobileApp.Converters;

// 騒音レベルを段階色に変換する(〜50 安全 / 〜70 注意 / 70〜 警告)
public sealed class DecibelToColorConverter : IValueConverter
{
    public double WarningThreshold { get; set; } = 50d;

    public double DangerThreshold { get; set; } = 70d;

    public Color SafeColor { get; set; } = Colors.LimeGreen;

    public Color WarningColor { get; set; } = Colors.Gold;

    public Color DangerColor { get; set; } = Colors.Red;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double decibel)
        {
            return SafeColor;
        }

        if (decibel >= DangerThreshold)
        {
            return DangerColor;
        }
        return decibel >= WarningThreshold ? WarningColor : SafeColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
