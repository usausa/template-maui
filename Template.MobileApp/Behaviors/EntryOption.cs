namespace Template.MobileApp.Behaviors;

#if ANDROID
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
#endif

#if ANDROID
using Java.Lang;
#endif

using Microsoft.Maui.Handlers;

public static class EntryOption
{
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

    public static void UseCustomMapper(BehaviorOptions options)
    {
#if ANDROID
        // HandleEnterKey
        if (options.HandleEnterKey)
        {
            EntryHandler.Mapper.Add(HandleEnterKeyProperty.PropertyName, static (handler, _) => UpdateHandleEnterKey(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.Add(HandleEnterKeyProperty.PropertyName, static (handler, _) => UpdateHandleEnterKey(handler.PlatformView, (Editor)handler.VirtualView));
        }

        // DisableShowSoftInputOnFocus
        if (options.DisableShowSoftInputOnFocus)
        {
            EntryHandler.Mapper.Add(DisableShowSoftInputOnFocusProperty.PropertyName, static (handler, _) => UpdateDisableShowSoftInputOnFocus(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.Add(DisableShowSoftInputOnFocusProperty.PropertyName, static (handler, _) => UpdateDisableShowSoftInputOnFocus(handler.PlatformView, (Editor)handler.VirtualView));
        }

        // SelectAllOnFocus
        if (options.SelectAllOnFocus)
        {
            EntryHandler.Mapper.Add(SelectAllOnFocusProperty.PropertyName, static (handler, _) => UpdateSelectAllOnFocus(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.Add(SelectAllOnFocusProperty.PropertyName, static (handler, _) => UpdateSelectAllOnFocus(handler.PlatformView, (Editor)handler.VirtualView));
        }

        // InputFilter
        if (options.InputFilter)
        {
            EntryHandler.Mapper.Add(InputFilterProperty.PropertyName, static (handler, _) => UpdateInputFilter(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.Add(InputFilterProperty.PropertyName, static (handler, _) => UpdateInputFilter(handler.PlatformView, (Editor)handler.VirtualView));
        }
#endif
    }

#if ANDROID
    private static void UpdateDisableShowSoftInputOnFocus(TextView editText, BindableObject element)
    {
        var value = GetDisableShowSoftInputOnFocus(element);
        editText.ShowSoftInputOnFocus = !value;
    }

    private static void UpdateSelectAllOnFocus(TextView editText, BindableObject element)
    {
        var value = GetSelectAllOnFocus(element);
        editText.SetSelectAllOnFocus(value);
    }

    private static void UpdateInputFilter(TextView editText, BindableObject element)
    {
        var rule = GetInputFilter(element);
        editText.SetFilters(rule is null ? Array.Empty<IInputFilter>() : new IInputFilter[] { new InputFilterInputFilter(rule) });
    }

    private sealed class InputFilterInputFilter : Java.Lang.Object, IInputFilter
    {
        private readonly Func<string, bool> rule;

        public InputFilterInputFilter(Func<string, bool> rule)
        {
            this.rule = rule;
        }

        public ICharSequence? FilterFormatted(ICharSequence? source, int start, int end, ISpanned? dest, int dstart, int dend)
        {
            var value = dest!.SubSequence(0, dstart) + source!.SubSequence(start, end) + dest!.SubSequence(dend, dest!.Length());
            return rule(value) ? source : new Java.Lang.String(dest.SubSequence(dstart, dend));
        }
    }

    private static void UpdateHandleEnterKey(EditText editText, BindableObject element)
    {
        var value = GetHandleEnterKey(element);
        if (value)
        {
            editText.EditorAction += OnEditorAction;
        }
    }

    private static void OnEditorAction(object? sender, Android.Widget.TextView.EditorActionEventArgs e)
    {
        if ((e.ActionId == ImeAction.ImeNull) && (e.Event?.KeyCode == Keycode.Enter))
        {
            e.Handled = true;
        }
    }
#endif
}
