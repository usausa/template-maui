namespace Template.MobileApp.Input;

public interface IInputHandler
{
    bool Handle(ShortcutKey key);

    VisualElement? FindFocused();
}
