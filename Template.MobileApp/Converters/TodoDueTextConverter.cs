namespace Template.MobileApp.Converters;

using Template.MobileApp.Models.App;

public sealed class TodoDueTextConverter : IMultiValueConverter
{
    public string TodayText { get; set; } = "今日";

    public string TomorrowText { get; set; } = "明日";

    public string DateFormat { get; set; } = "M/d (ddd)";

    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture) =>
        values is [TodoDue due, DateTime date, ..]
            ? due switch
            {
                TodoDue.Today => TodayText,
                TodoDue.Tomorrow => TomorrowText,
                _ => date.ToString(DateFormat, culture)
            }
            : string.Empty;

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
