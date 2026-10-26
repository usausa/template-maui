namespace Template.MobileApp.Converters;

using Template.MobileApp.Modules;

public sealed class TileSizeConverter : IValueConverter
{
    public double Padding { get; set; }

    public double Spacing { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value is int columns) && (columns > 0)
            ? ((ScreenSize.Width - (Padding * 2)) / columns) - Spacing
            : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
