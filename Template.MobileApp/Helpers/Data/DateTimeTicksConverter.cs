namespace Template.MobileApp.Helpers.Data;

using Smart.Data.Accessor.Converters;

public sealed class DateTimeTicksConverter : IValueConverter<long, DateTime>
{
    public static DateTime FromDb(long dbValue) => new(dbValue, DateTimeKind.Utc);

    public static long ToDb(DateTime clrValue) =>
        clrValue.Kind == DateTimeKind.Local ? clrValue.ToUniversalTime().Ticks : clrValue.Ticks;
}
