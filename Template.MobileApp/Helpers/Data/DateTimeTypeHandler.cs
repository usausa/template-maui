namespace Template.MobileApp.Helpers.Data;

using Smart.Data.Mapper.Handlers;

public sealed class DateTimeTypeHandler : TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.DbType = DbType.Int64;
        parameter.Value = value.Kind == DateTimeKind.Local ? value.ToUniversalTime().Ticks : value.Ticks;
    }

    public override DateTime Parse(object value)
    {
        return new((long)value, DateTimeKind.Utc);
    }
}
