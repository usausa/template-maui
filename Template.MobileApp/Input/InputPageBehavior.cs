namespace Template.MobileApp.Input;

using Smart.Maui.Interactivity;

using Template.MobileApp.Helpers;

public sealed class InputPageBehavior : BehaviorBase<Page>, IInputHandler
{
    protected override void OnAttachedTo(Page bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.Appearing += BindableOnAppearing;
        bindable.Disappearing += BindableOnDisappearing;
    }

    protected override void OnDetachingFrom(Page bindable)
    {
        bindable.Appearing -= BindableOnAppearing;
        bindable.Disappearing -= BindableOnDisappearing;
        base.OnDetachingFrom(bindable);
    }

    private void BindableOnAppearing(object? sender, EventArgs e)
    {
        InputManager.Default.PushHandler(this);
    }

    private void BindableOnDisappearing(object? sender, EventArgs e)
    {
        InputManager.Default.PopHandler(this);
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
