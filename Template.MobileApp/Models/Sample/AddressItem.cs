namespace Template.MobileApp.Models.Sample;

public sealed class AddressItem
{
    public string Image { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string PhoneNumber { get; set; } = default!;

    public string MailAddress { get; set; } = default!;
}

public sealed class AddressRow : AlternateRow<AddressItem>
{
    public AddressRow(AddressItem value)
        : base(value)
    {
    }
}

public sealed class AddressGroup : CollectionGroup<string, AddressRow>
{
    public AddressGroup(string key)
        : base(key)
    {
    }

    public AddressGroup(string key, IEnumerable<AddressRow> items, bool isExpanded = true)
        : base(key, items, isExpanded)
    {
    }
}
