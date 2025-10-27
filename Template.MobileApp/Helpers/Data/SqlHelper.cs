namespace Template.MobileApp.Helpers.Data;

using System.Reflection;

using Smart;
using Smart.Data.Mapper.Attributes;
using Smart.Data.Mapper.Builders.Metadata;

public static class SqlHelper
{
    private static readonly Dictionary<Type, string> TypeMap = new()
    {
        { typeof(string), "TEXT" },
        { typeof(Guid), "TEXT" },
        { typeof(DateTime), "INTEGER" },
        { typeof(long), "INTEGER" },
        { typeof(int), "INTEGER" },
        { typeof(double), "REAL" },
        { typeof(bool), "INTEGER" }
    };

    public static string MakeCreate<T>()
    {
        var table = TableInfo<T>.Instance;

        var sql = new StringBuilder();

        sql.Append("CREATE TABLE ");
        sql.Append(table.Name);
        sql.Append(" (");

        foreach (var column in table.Columns)
        {
            sql.Append(column.Name);
            sql.Append(' ');

            var isNullable = column.Property.PropertyType.IsNullableType();
            var propertyType = isNullable ? Nullable.GetUnderlyingType(column.Property.PropertyType) : column.Property.PropertyType;

            if ((propertyType is null) || !TypeMap.TryGetValue(propertyType, out var type))
            {
                throw new NotSupportedException($"Type not supported. type=[{column.Property.PropertyType}]");
            }

            sql.Append(type);

            if ((propertyType.IsValueType && !isNullable) ||
                (column.Property.GetCustomAttribute<NullableAttribute>() is null) ||
                (column.Property.GetCustomAttribute<PrimaryKeyAttribute>() is not null))
            {
                sql.Append(" NOT NULL");
            }

            sql.Append(", ");
        }

        if (table.KeyColumns.Count > 0)
        {
            sql.Append("PRIMARY KEY (");

            foreach (var column in table.KeyColumns)
            {
                sql.Append(column.Name);
                sql.Append(", ");
            }

            sql.Length -= 2;
            sql.Append(')');
        }
        else
        {
            sql.Length -= 2;
        }

        sql.Append(')');

        return sql.ToString();
    }
}
