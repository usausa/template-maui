namespace Template.MobileApp.Models;

public sealed partial class Selectable<T> : ObservableObject
{
    public T Item { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public Selectable(T item)
    {
        Item = item;
    }
}
