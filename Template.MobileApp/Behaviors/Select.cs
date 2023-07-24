namespace Template.MobileApp.Behaviors;

using System.Collections.Generic;
using System.Linq;

public static class Select
{
    public static readonly BindableProperty ListProperty = BindableProperty.CreateAttached(
        "List",
        typeof(ICollection<SelectItem>),
        typeof(SelectItem),
        null,
        propertyChanged: HandlePropertyChanged);

    public static readonly BindableProperty ValueProperty = BindableProperty.CreateAttached(
        "Value",
        typeof(object),
        typeof(SelectItem),
        null,
        propertyChanged: HandlePropertyChanged);

    public static readonly BindableProperty EmptyStringProperty = BindableProperty.CreateAttached(
        "EmptyString",
        typeof(string),
        typeof(SelectItem),
        null,
        propertyChanged: HandlePropertyChanged);

    public static List<SelectItem>? GetList(BindableObject view) => (List<SelectItem>)view.GetValue(ListProperty);

    public static void SetList(BindableObject view, List<SelectItem>? value) => view.SetValue(ListProperty, value);

    public static object? GetValue(BindableObject view) => (string)view.GetValue(ValueProperty);

    public static void SetValue(BindableObject view, object? value) => view.SetValue(ValueProperty, value);

    public static string? GetEmptyString(BindableObject view) => (string?)view.GetValue(EmptyStringProperty);

    public static void SetEmptyString(BindableObject view, string? value) => view.SetValue(EmptyStringProperty, value);

    private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var list = GetList(bindable);
        if (list is null)
        {
            return;
        }

        var key = GetValue(bindable);
        var entity = list.FirstOrDefault(x => Equals(x.Key, key));
        if (entity is null)
        {
            var text = GetEmptyString(bindable) ?? string.Empty;
            if (bindable is Button button)
            {
                button.Text = text;
            }
            else if (bindable is Label label)
            {
                label.Text = text;
            }
        }
        else
        {
            if (bindable is Button button)
            {
                button.Text = entity.Name;
            }
            else if (bindable is Label label)
            {
                label.Text = entity.Name;
            }
        }
    }
}
