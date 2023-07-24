namespace Template.MobileApp.Behaviors;

#if ANDROID
using Android.Graphics.Drawables;
using Android.Text;
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

    public static void UseCustomMapper(BehaviorOptions options)
    {
#if ANDROID
        // DisableShowSoftInputOnFocus
        if (options.DisableShowSoftInputOnFocus)
        {
            EntryHandler.Mapper.Add("DisableShowSoftInputOnFocus", static (handler, _) => UpdateDisableShowSoftInputOnFocus(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.Add("DisableShowSoftInputOnFocus", static (handler, _) => UpdateDisableShowSoftInputOnFocus(handler.PlatformView, (Editor)handler.VirtualView));
        }

        // SelectAllOnFocus
        if (options.SelectAllOnFocus)
        {
            EntryHandler.Mapper.Add("SelectAllOnFocus", static (handler, _) => UpdateSelectAllOnFocus(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.Add("SelectAllOnFocus", static (handler, _) => UpdateSelectAllOnFocus(handler.PlatformView, (Editor)handler.VirtualView));
        }

        // NoBorder
        if (options.NoBorder)
        {
            EntryHandler.Mapper.Add("NoBorder", static (handler, _) => UpdateNoBorder((Entry)handler.VirtualView));
            EditorHandler.Mapper.Add("NoBorder", static (handler, _) => UpdateNoBorder((Editor)handler.VirtualView));
        }

        // InputFilter
        if (options.InputFilter)
        {
            EntryHandler.Mapper.Add("InputFilter", static (handler, _) => UpdateInputFilter(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.Add("InputFilter", static (handler, _) => UpdateInputFilter(handler.PlatformView, (Editor)handler.VirtualView));
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

    private static void UpdateNoBorder(VisualElement element)
    {
        var on = GetNoBorder(element);
        if (on)
        {
            element.Behaviors.Add(new NoBorderBehavior());
        }
        else
        {
            var behavior = element.Behaviors.FirstOrDefault(static x => x is NoBorderBehavior);
            if (behavior is not null)
            {
                element.Behaviors.Remove(behavior);
            }
        }
    }

    private sealed class NoBorderBehavior : PlatformBehavior<InputView, EditText>
    {
        private Drawable? originalDrawable;

        protected override void OnAttachedTo(InputView bindable, EditText platformView)
        {
            base.OnAttachedTo(bindable, platformView);

            originalDrawable = platformView.Background;
            // TODO
            platformView.Background = null;
            platformView.SetPadding(0, 0, 0, 0);
        }

        protected override void OnDetachedFrom(InputView bindable, EditText platformView)
        {
            platformView.Background = originalDrawable;

            base.OnDetachedFrom(bindable, platformView);
        }
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
#endif
}
