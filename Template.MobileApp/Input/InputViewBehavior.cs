namespace Template.MobileApp.Input;

using Smart.Maui.Interactivity;

using Template.MobileApp.Helpers;

public sealed class InputPopupBehavior : BehaviorBase<View>, IInputHandler
{
    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        InputManager.Default.PushHandler(this);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        InputManager.Default.PopHandler(this);
        base.OnDetachingFrom(bindable);
    }

    public bool Handle(ShortcutKey key)
    {
        if ((AssociatedObject is null) || !AssociatedObject.IsEnabled)
        {
            return false;
        }

        var focused = FindFocused();
        if (focused is not null)
        {
            if (focused.Behaviors.OfType<IShortcutBehavior>().Any(x => x.Handle(key)))
            {
                return true;
            }
        }

        if (key == ShortcutKey.Up)
        {
            ElementHelper.MoveFocus(AssociatedObject, false);
            return true;
        }

        if (key == ShortcutKey.Down)
        {
            ElementHelper.MoveFocus(AssociatedObject, true);
            return true;
        }

        var button = ElementHelper.EnumerateActive<Button>(AssociatedObject)
            .FirstOrDefault(key, static (x, s) => Shortcut.GetKey(x) == s);
        if (button is not null)
        {
            button.SendClicked();
            return true;
        }

        return false;
    }

    public VisualElement? FindFocused() =>
        AssociatedObject is not null ? ElementHelper.FindFocused(AssociatedObject) : null;
}
