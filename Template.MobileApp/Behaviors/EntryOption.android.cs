namespace Template.MobileApp.Behaviors;

using Android.Content.Res;
using Android.Text;
using Android.Views;
using Android.Widget;

using Java.Lang;

using Microsoft.Maui.Handlers;

public static partial class EntryOption
{
    //--------------------------------------------------------------------------------
    // Mapper
    //--------------------------------------------------------------------------------

    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        // HandleEnterKey
        if (options.HandleEnterKey)
        {
            EntryHandler.Mapper.AppendToMapping(HandleEnterKeyProperty.PropertyName, static (handler, _) => UpdateHandleEnterKey(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.AppendToMapping(HandleEnterKeyProperty.PropertyName, static (handler, _) => UpdateHandleEnterKey(handler.PlatformView, (Editor)handler.VirtualView));
        }

        // DisableShowSoftInputOnFocus
        if (options.DisableShowSoftInputOnFocus)
        {
            EntryHandler.Mapper.AppendToMapping(DisableShowSoftInputOnFocusProperty.PropertyName, static (handler, _) => UpdateDisableShowSoftInputOnFocus(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.AppendToMapping(DisableShowSoftInputOnFocusProperty.PropertyName, static (handler, _) => UpdateDisableShowSoftInputOnFocus(handler.PlatformView, (Editor)handler.VirtualView));
        }

        // SelectAllOnFocus
        if (options.SelectAllOnFocus)
        {
            EntryHandler.Mapper.AppendToMapping(SelectAllOnFocusProperty.PropertyName, static (handler, _) => UpdateSelectAllOnFocus(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.AppendToMapping(SelectAllOnFocusProperty.PropertyName, static (handler, _) => UpdateSelectAllOnFocus(handler.PlatformView, (Editor)handler.VirtualView));
        }

        // NoBorder
        if (options.NoBorder)
        {
            EntryHandler.Mapper.AppendToMapping(NoBorderProperty.PropertyName, static (handler, _) => UpdateHandleNoBorder(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.AppendToMapping(NoBorderProperty.PropertyName, static (handler, _) => UpdateHandleNoBorder(handler.PlatformView, (Editor)handler.VirtualView));
            PickerHandler.Mapper.AppendToMapping(NoBorderProperty.PropertyName, static (handler, _) => UpdateHandleNoBorder(handler.PlatformView, (Picker)handler.VirtualView));
            DatePickerHandler.Mapper.AppendToMapping(NoBorderProperty.PropertyName, static (handler, _) => UpdateHandleNoBorder(handler.PlatformView, (BindableObject)handler.VirtualView));
            TimePickerHandler.Mapper.AppendToMapping(NoBorderProperty.PropertyName, static (handler, _) => UpdateHandleNoBorder(handler.PlatformView, (BindableObject)handler.VirtualView));
            SearchBarHandler.Mapper.AppendToMapping(NoBorderProperty.PropertyName, static (handler, _) => UpdateSearchBarNoBorder(handler.PlatformView, (SearchBar)handler.VirtualView));
        }

        // InputFilter
        if (options.InputFilter)
        {
            EntryHandler.Mapper.AppendToMapping(InputFilterProperty.PropertyName, static (handler, _) => UpdateInputFilter(handler.PlatformView, (Entry)handler.VirtualView));
            EditorHandler.Mapper.AppendToMapping(InputFilterProperty.PropertyName, static (handler, _) => UpdateInputFilter(handler.PlatformView, (Editor)handler.VirtualView));
        }
    }

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

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

    private static void UpdateHandleNoBorder(TextView editText, BindableObject element)
    {
        if (GetNoBorder(element))
        {
            editText.BackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        }
    }

    private static void UpdateSearchBarNoBorder(View searchView, BindableObject element)
    {
        var plate = searchView.FindViewById(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.search_plate);
        if (plate is not null)
        {
            var value = GetNoBorder(element);
            plate.BackgroundTintList = value ? ColorStateList.ValueOf(Android.Graphics.Color.Transparent) : null;
        }
    }

    private static void UpdateInputFilter(TextView editText, BindableObject element)
    {
        var rule = GetInputFilter(element);
#pragma warning disable CA2000
        editText.SetFilters(rule is null ? [] : [new InputFilterInputFilter(rule)]);
#pragma warning restore CA2000
    }

    //--------------------------------------------------------------------------------
    // Input Filter
    //--------------------------------------------------------------------------------

    private sealed class InputFilterInputFilter : Object, IInputFilter
    {
        private readonly Func<string, bool> rule;

        public InputFilterInputFilter(Func<string, bool> rule)
        {
            this.rule = rule;
        }

        public ICharSequence? FilterFormatted(ICharSequence? source, int start, int end, ISpanned? dest, int dstart, int dend)
        {
            var value = dest!.SubSequence(0, dstart) + source!.SubSequence(start, end) + dest!.SubSequence(dend, dest!.Length());
            return rule(value) ? source : new String(dest.SubSequence(dstart, dend));
        }
    }

    private static void UpdateHandleEnterKey(EditText editText, BindableObject element)
    {
        var value = GetHandleEnterKey(element);

        editText.EditorAction -= OnEditorAction;
        if (value)
        {
            editText.EditorAction += OnEditorAction;
        }
    }

    private static void OnEditorAction(object? sender, TextView.EditorActionEventArgs e)
    {
        if (e.Event?.KeyCode == Keycode.Enter)
        {
            e.Handled = true;
        }
    }
}
