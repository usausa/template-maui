namespace Template.MobileApp.Helpers.Data;

using Smart.Data.Mapper.Handlers;

public sealed class GuidTypeHandler : TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString();
    }

    public override Guid Parse(object value)
    {
        return Guid.Parse((string)value);
    }
}
