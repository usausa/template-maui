namespace Template.MobileApp.Behaviors;

using System.Collections.Generic;

public static class Select
{
    public static readonly BindableProperty ListProperty = BindableProperty.CreateAttached(
        "List",
        typeof(IEnumerable<SelectItem>),
        typeof(Select),
        null,
        propertyChanged: HandlePropertyChanged);

    public static readonly BindableProperty ValueProperty = BindableProperty.CreateAttached(
        "Value",
        typeof(object),
        typeof(Select),
        null,
        propertyChanged: HandlePropertyChanged);

    public static readonly BindableProperty EmptyStringProperty = BindableProperty.CreateAttached(
        "EmptyString",
        typeof(string),
        typeof(Select),
        null,
        propertyChanged: HandlePropertyChanged);

    public static IEnumerable<SelectItem?>? GetList(BindableObject obj) => (IEnumerable<SelectItem>)obj.GetValue(ListProperty);

    public static void SetList(BindableObject obj, IEnumerable<SelectItem?>? value) => obj.SetValue(ListProperty, value);

    public static object? GetValue(BindableObject obj) => obj.GetValue(ValueProperty);

    public static void SetValue(BindableObject obj, object? value) => obj.SetValue(ValueProperty, value);

    public static string? GetEmptyString(BindableObject obj) => (string?)obj.GetValue(EmptyStringProperty);

    public static void SetEmptyString(BindableObject obj, string? value) => obj.SetValue(EmptyStringProperty, value);

    private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var list = GetList(bindable);
        if (list is null)
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
            return;
        }

        var key = GetValue(bindable);
        var entity = list.FirstOrDefault(key, static (x, s) => Equals(x?.Key, s));
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
