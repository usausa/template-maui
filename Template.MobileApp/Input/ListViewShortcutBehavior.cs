namespace Template.MobileApp.Input;

using Smart.Maui.Interactivity;

public sealed class ListViewShortcutBehavior : BehaviorBase<ListView>, IShortcutBehavior
{
    public static readonly BindableProperty ShortcutProperty = BindableProperty.Create(
        nameof(Shortcut),
        typeof(ShortcutKey),
        typeof(ListViewShortcutBehavior));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(ListViewShortcutBehavior));

    public ShortcutKey Shortcut
    {
        get => (ShortcutKey)GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public bool Handle(ShortcutKey key)
    {
        if ((Shortcut != key) || (AssociatedObject is null))
        {
            return false;
        }

        var command = Command;
        if (command is null)
        {
            return false;
        }

        var item = AssociatedObject.SelectedItem;
        if ((item is not null) && command.CanExecute(item))
        {
            command.Execute(item);
        }

        return true;
    }
}
