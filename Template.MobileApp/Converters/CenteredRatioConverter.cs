namespace Template.MobileApp.Converters;

// ±Range の値を 0〜1 に正規化する(中央=0.5)。センサー値のレベルバー表示用
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
