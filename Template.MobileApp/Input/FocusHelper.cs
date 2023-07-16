namespace Template.MobileApp.Input;

using System;

public static class FocusHelper
{
    public static async ValueTask<T> WithRestoreFocus<T>(Func<ValueTask<T>> func)
    {
        var focused = InputManager.Default.FindFocused();

        var result = await func();

        if (focused is not null)
        {
#pragma warning disable CA1849
            Application.Current?.Dispatcher.Dispatch(() => focused.Focus());
#pragma warning restore CA1849
        }

        return result;
    }

    public static async ValueTask WithRestoreFocus(Func<ValueTask> func)
    {
        var focused = InputManager.Default.FindFocused();

        await func();

        if (focused is not null)
        {
#pragma warning disable CA1849
            Application.Current?.Dispatcher.Dispatch(() => focused.Focus());
#pragma warning restore CA1849
        }
    }
}
