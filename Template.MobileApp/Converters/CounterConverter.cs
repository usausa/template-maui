namespace Template.MobileApp.Converters;

using System;
using System.Globalization;

public sealed class CounterConverter : IMultiValueConverter
{
    public object? Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if ((values is null) || (values.Length < 2))
        {
            return null;
        }

        var current = System.Convert.ToInt32(values[0]);
        var max = System.Convert.ToInt32(values[1]);
        return $"{current:D3}/{max:D3}";
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
