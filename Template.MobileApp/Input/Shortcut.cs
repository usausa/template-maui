namespace Template.MobileApp.Input;

public static class Shortcut
{
    public static readonly BindableProperty KeyProperty = BindableProperty.CreateAttached(
        "Key",
        typeof(ShortcutKey),
        typeof(Shortcut),
        null);

    public static ShortcutKey GetKey(BindableObject bindable) => (ShortcutKey)bindable.GetValue(KeyProperty);

    public static void SetKey(BindableObject bindable, ShortcutKey value) => bindable.SetValue(KeyProperty, value);
}
