namespace Template.MobileApp.Converters;

using Microsoft.Maui.Layouts;

public sealed class BasisConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        new FlexBasis((float)Math.Clamp(value is double rate ? rate : 0d, 0d, 1d), true);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
