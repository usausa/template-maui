namespace Template.MobileApp.Converters;

public sealed class CenteredRatioConverter : IValueConverter
{
    public double Range { get; set; } = 1d;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var v = value switch
        {
            double d => d,
            float f => f,
            int i => i,
            _ => 0d
        };
        return Math.Clamp((v / (Range * 2)) + 0.5, 0d, 1d);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
