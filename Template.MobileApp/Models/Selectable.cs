namespace Template.MobileApp.Models;

using Smart.ComponentModel;

public class Selectable<T> : NotificationObject
{
    private bool isSelected;

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    public T Value { get; }

    public Selectable(T value)
    {
        Value = value;
    }
}
