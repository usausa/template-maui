namespace Template.MobileApp.Converters;

public enum ClockPart
{
    All,
    Main,
    Fraction
}

public sealed class ClockTextConverter : IValueConverter
{
    public ClockPart Part { get; set; }

    public bool RoundUp { get; set; }

    public bool Unlit { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan time)
        {
            return value;
        }

        if (RoundUp)
        {
            time = TimeSpan.FromSeconds(Math.Ceiling(time.TotalSeconds));
        }

        var main = time.TotalHours >= 1
            ? $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}"
            : $"{time.Minutes:D2}:{time.Seconds:D2}";
        var fraction = $".{time.Milliseconds / 10:D2}";
        var text = Part switch
        {
            ClockPart.Main => main,
            ClockPart.Fraction => fraction,
            _ => main + fraction
        };
        return Unlit ? ToUnlit(text) : text;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static string ToUnlit(string text) =>
        String.Create(text.Length, text, static (span, source) =>
        {
            for (var i = 0; i < source.Length; i++)
            {
                span[i] = Char.IsAsciiDigit(source[i]) ? '8' : source[i];
            }
        });
}
