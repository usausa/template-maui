namespace Template.MobileApp.Models;

public sealed class SelectItem
{
    public object Key { get; }

    public string Name { get; }

    public SelectItem(object key, string name)
    {
        Key = key;
        Name = name;
    }
}
