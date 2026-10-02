namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

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

    // 入力が止まってから TypingStoppedDelay ミリ秒後に、入力中の文字列を引数にしてコマンドを実行する
    public static readonly BindableProperty TypingStoppedCommandProperty = BindableProperty.CreateAttached(
        "TypingStoppedCommand",
        typeof(ICommand),
        typeof(EntryOption),
        null,
        propertyChanged: OnTypingStoppedCommandChanged);

    public static ICommand? GetTypingStoppedCommand(BindableObject bindable) => (ICommand?)bindable.GetValue(TypingStoppedCommandProperty);

    public static void SetTypingStoppedCommand(BindableObject bindable, ICommand? value) => bindable.SetValue(TypingStoppedCommandProperty, value);

    public static readonly BindableProperty TypingStoppedDelayProperty = BindableProperty.CreateAttached(
        "TypingStoppedDelay",
        typeof(int),
        typeof(EntryOption),
        1000,
        propertyChanged: OnTypingStoppedDelayChanged);

    public static int GetTypingStoppedDelay(BindableObject bindable) => (int)bindable.GetValue(TypingStoppedDelayProperty);

    public static void SetTypingStoppedDelay(BindableObject bindable, int value) => bindable.SetValue(TypingStoppedDelayProperty, value);

    private static void OnTypingStoppedCommandChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not InputView view)
        {
            return;
        }

        var behavior = view.Behaviors.OfType<TypingStoppedBehavior>().FirstOrDefault();
        if (newValue is ICommand command)
        {
            if (behavior is null)
            {
                behavior = new TypingStoppedBehavior { Delay = TimeSpan.FromMilliseconds(GetTypingStoppedDelay(view)) };
                view.Behaviors.Add(behavior);
            }

            behavior.Command = command;
        }
        else if (behavior is not null)
        {
            view.Behaviors.Remove(behavior);
        }
    }

    private static void OnTypingStoppedDelayChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if ((bindable is InputView view) && (view.Behaviors.OfType<TypingStoppedBehavior>().FirstOrDefault() is { } behavior))
        {
            behavior.Delay = TimeSpan.FromMilliseconds((int)newValue!);
        }
    }
}
