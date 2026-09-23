namespace Template.MobileApp.Converters;

public sealed class FlagsTextEntry
{
    public Enum Key { get; set; } = default!;

    public string Value { get; set; } = string.Empty;
}

// [Flags] の列挙値を、立っているビットの文言を Entries の順につないだ文字列へ
public sealed class FlagsToTextConverter : IValueConverter
{
    public IList<FlagsTextEntry> Entries { get; } = [];

    public string Separator { get; set; } = string.Empty;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum flags)
        {
            return string.Empty;
        }

        var bits = System.Convert.ToInt64(flags, culture);
        return String.Join(Separator, Entries.Where(x => (bits & System.Convert.ToInt64(x.Key, culture)) != 0).Select(static x => x.Value));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
