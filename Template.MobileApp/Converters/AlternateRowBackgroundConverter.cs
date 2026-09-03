namespace Template.MobileApp.Converters;

public sealed class AlternateRowBackgroundConverter : IValueConverter
{
    public Color EvenColor { get; set; } = Colors.Transparent;

    public Color OddColor { get; set; } = Colors.Transparent;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value is int row) && ((row % 2) != 0) ? OddColor : EvenColor;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
