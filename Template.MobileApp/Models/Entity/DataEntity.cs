namespace Template.MobileApp.Models.Entity;

using Smart.Data.Accessor.Attributes;

using Template.MobileApp.Helpers.Data;

[Name("Data")]
public sealed class DataEntity
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    [TypeHandler(typeof(DateTimeTicksConverter))]
    public DateTime CreateAt { get; set; }
}
