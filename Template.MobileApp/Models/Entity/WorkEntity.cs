namespace Template.MobileApp.Models.Entity;

using Smart.Data.Mapper.Attributes;

public class WorkEntity
{
    [PrimaryKey]
    public long Id { get; set; }

    public string Name { get; set; } = default!;
}
