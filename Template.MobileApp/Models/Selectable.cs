namespace Template.MobileApp.Models;

using Smart.ComponentModel;

public class Selectable<T> : NotificationObject
{
    public bool IsSelected
    {
        get;
        set => SetProperty(ref field, value);
    }

    public T Value { get; }

    public Selectable(T value)
    {
        Value = value;
    }
}
