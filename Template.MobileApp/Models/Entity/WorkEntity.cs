namespace Template.MobileApp.Models.Entity;

using Smart.Data.Accessor.Attributes;

[Name("Work")]
public sealed class WorkEntity
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = default!;
}
