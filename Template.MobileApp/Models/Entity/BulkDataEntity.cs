namespace Template.MobileApp.Models.Entity;

using Smart.Data.Mapper.Attributes;

public sealed class BulkDataEntity
{
    [PrimaryKey(1)]
    public string Key1 { get; set; } = default!;

    [PrimaryKey(2)]
    public string Key2 { get; set; } = default!;

    [PrimaryKey(3)]
    public string Key3 { get; set; } = default!;

    public int Value1 { get; set; }

    public int Value2 { get; set; }

    public int Value3 { get; set; }

    public int Value4 { get; set; }

    public int Value5 { get; set; }
}
