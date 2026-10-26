namespace Template.MobileApp.Converters;

public sealed class WipTextConverter : IMultiValueConverter
{
    public object Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [int limit, bool isOver])
        {
            return string.Empty;
        }

        if (limit <= 0)
        {
            return "WIP 制限なし";
        }

        return String.Format(culture, isOver ? "WIP 上限 {0} を超過" : "WIP 上限 {0}", limit);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
