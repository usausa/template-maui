namespace Template.MobileApp.Converters;

// 残量(0〜1)の言葉は 85% 以上 十分・60% 以上 良好・30% 以上 普通・それ未満 少ない、色は 60% 以上・30% 以上・それ未満の 3 段
public sealed class HomeBatteryLevelConverter : IValueConverter
{
    public bool IsColor { get; set; }

    public Color HighColor { get; set; } = Colors.LimeGreen;

    public Color MediumColor { get; set; } = Colors.Orange;

    public Color LowColor { get; set; } = Colors.Red;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var level = value is double d ? d : 0d;
        return IsColor
            ? level switch
            {
                >= 0.6 => HighColor,
                >= 0.3 => MediumColor,
                _ => LowColor
            }
            : level switch
            {
                >= 0.85 => "十分",
                >= 0.6 => "良好",
                >= 0.3 => "普通",
                _ => "少ない"
            };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
