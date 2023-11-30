#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Template.MobileApp.Input;

using Android.App;
using Android.Views;
using Android.Widget;

public sealed class KeyInputDriver
{
    private static readonly ConvertEntry[] OtherEntries =
    [
        new(Keycode.Del, ShortcutKey.Del),
        new(Keycode.Minus, ShortcutKey.Minus),
        new(Keycode.Period, ShortcutKey.Period)
    ];

    private readonly Activity activity;

    public KeyInputDriver(Activity activity)
    {
        this.activity = activity;
    }

    private sealed class ConvertEntry
    {
        public Keycode AndroidKeycode { get; }

        public ShortcutKey InputKeyCode { get; }

        public ConvertEntry(Keycode androidKeycode, ShortcutKey inputKeyCode)
        {
            AndroidKeycode = androidKeycode;
            InputKeyCode = inputKeyCode;
        }
    }

    public bool Process(KeyEvent e)
    {
        // ↑
        if (e.KeyCode == Keycode.DpadUp)
        {
            // TODO check
            // MEMO 1 is header
            if ((activity.CurrentFocus is ListView listView) && (listView.SelectedItemPosition > 1))
            {
                return false;
            }

            if (e.Action == KeyEventActions.Down)
            {
                InputManager.Default.Process(ShortcutKey.Up);
            }

            return true;
        }

        // ↓
        if (e.KeyCode == Keycode.DpadDown)
        {
            // TODO check
            // MEMO 2 is header and footer
            if ((activity.CurrentFocus is ListView listView) && (listView.SelectedItemPosition < listView.Adapter!.Count - 2))
            {
                return false;
            }

            if (e.Action == KeyEventActions.Down)
            {
                InputManager.Default.Process(ShortcutKey.Down);
            }

            return true;
        }

        // ←
        if (e.KeyCode == Keycode.DpadLeft)
        {
            // TODO check
            if (activity.CurrentFocus is EditText editText)
            {
                // first position
                if ((editText.SelectionStart == 0) && (editText.SelectionEnd == 0))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        // →
        if (e.KeyCode == Keycode.DpadRight)
        {
            // TODO check
            if (activity.CurrentFocus is EditText editText)
            {
                // last position
                var textLength = editText.Text?.Length ?? 0;
                if ((editText.SelectionStart == textLength) && (editText.SelectionEnd == textLength))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        // Number
        if ((e.KeyCode >= Keycode.Num0) && (e.KeyCode <= Keycode.Num9))
        {
            if (activity.CurrentFocus is EditText)
            {
                return false;
            }

            if (e.Action == KeyEventActions.Up)
            {
                InputManager.Default.Process((ShortcutKey)((int)ShortcutKey.Num0 + (e.KeyCode - Keycode.Num0)));
            }

            return true;
        }

        // Function
        if ((e.KeyCode >= Keycode.F1) && (e.KeyCode <= Keycode.F12))
        {
            if (e.Action == KeyEventActions.Up)
            {
                InputManager.Default.Process((ShortcutKey)((int)ShortcutKey.Function1 + (e.KeyCode - Keycode.F1)));
            }

            return true;
        }

        // Others
        foreach (var entry in OtherEntries)
        {
            if (e.KeyCode == entry.AndroidKeycode)
            {
                if (activity.CurrentFocus is EditText)
                {
                    return false;
                }

                if (e.Action == KeyEventActions.Up)
                {
                    InputManager.Default.Process(entry.InputKeyCode);
                }

                return true;
            }
        }

        return false;
    }
}
