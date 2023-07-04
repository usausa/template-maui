namespace Template.MobileApp.Behaviors;

public static class Focus
{
    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty DefaultProperty = BindableProperty.CreateAttached(
        "Default",
        typeof(bool),
        typeof(Focus),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetDefault(BindableObject bindable) => (bool)bindable.GetValue(DefaultProperty);

    public static void SetDefault(BindableObject bindable, bool value) => bindable.SetValue(DefaultProperty, value);
}
