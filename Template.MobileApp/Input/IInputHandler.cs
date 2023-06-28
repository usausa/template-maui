namespace Template.MobileApp.Input;

public interface IInputHandler
{
    bool Handle(KeyCode key);

    VisualElement? FindFocused();
}
