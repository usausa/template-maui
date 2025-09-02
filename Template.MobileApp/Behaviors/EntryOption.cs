namespace Template.MobileApp.Behaviors;

public static partial class EntryOption
{
    public static partial void UseCustomMapper(BehaviorOptions options);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty DisableShowSoftInputOnFocusProperty = BindableProperty.CreateAttached(
        "DisableShowSoftInputOnFocus",
        typeof(bool),
        typeof(EntryOption),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetDisableShowSoftInputOnFocus(BindableObject bindable) => (bool)bindable.GetValue(DisableShowSoftInputOnFocusProperty);

    public static void SetDisableShowSoftInputOnFocus(BindableObject bindable, bool value) => bindable.SetValue(DisableShowSoftInputOnFocusProperty, value);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty SelectAllOnFocusProperty = BindableProperty.CreateAttached(
        "SelectAllOnFocus",
        typeof(bool),
        typeof(EntryOption),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetSelectAllOnFocus(BindableObject bindable) => (bool)bindable.GetValue(SelectAllOnFocusProperty);

    public static void SetSelectAllOnFocus(BindableObject bindable, bool value) => bindable.SetValue(SelectAllOnFocusProperty, value);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty NoBorderProperty = BindableProperty.CreateAttached(
        "NoBorder",
        typeof(bool),
        typeof(EntryOption),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetNoBorder(BindableObject bindable) => (bool)bindable.GetValue(NoBorderProperty);

    public static void SetNoBorder(BindableObject bindable, bool value) => bindable.SetValue(NoBorderProperty, value);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty InputFilterProperty = BindableProperty.CreateAttached(
        "InputFilter",
        typeof(Func<string, bool>),
        typeof(EntryOption),
        null);
    // ReSharper restore InconsistentNaming

    public static Func<string, bool>? GetInputFilter(BindableObject bindable) => (Func<string, bool>?)bindable.GetValue(InputFilterProperty);

    public static void SetInputFilter(BindableObject bindable, Func<string, bool>? value) => bindable.SetValue(InputFilterProperty, value);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty HandleEnterKeyProperty = BindableProperty.CreateAttached(
        "HandleEnterKey",
        typeof(bool),
        typeof(EntryOption),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetHandleEnterKey(BindableObject bindable) => (bool)bindable.GetValue(HandleEnterKeyProperty);
    public static void SetHandleEnterKey(BindableObject bindable, bool value) => bindable.SetValue(HandleEnterKeyProperty, value);
}
