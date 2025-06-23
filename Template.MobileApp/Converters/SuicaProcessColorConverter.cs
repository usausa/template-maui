namespace Template.MobileApp.Converters;

public class ProcessColor
{
    public int ProcessType { get; set; }

    public Color Color { get; set; } = Colors.Gray;
}

public sealed class SuicaProcessColorConverter : IValueConverter
{
#pragma warning disable CA1819
    public ProcessColor[] Values { get; set; } = [];
#pragma warning restore CA1819

    public Color DefaultColor { get; set; } = Colors.Gray;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte byteValue)
        {
            var processType = (int)SuicaLogic.ConvertProcessType(byteValue);
            foreach (var color in Values)
            {
                if (color.ProcessType == processType)
                {
                    return color.Color;
                }
            }
        }

        return DefaultColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
