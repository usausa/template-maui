namespace Template.MobileApp.Shell;

using Microsoft.Maui.Controls.Shapes;

using Smart.Maui.Interactivity;

public static class ShellProperty
{
    // 現在表示中のビュー。遷移で退場するビューのバインディング解除がプロパティ変更を発火させ、
    // 直後のシェル状態を旧値で上書きするのを防ぐ (現在のビューからの変更のみ反映する)
    private static WeakReference<BindableObject>? currentView;

    internal static void SetCurrentView(BindableObject? view)
    {
        currentView = view is null ? null : new WeakReference<BindableObject>(view);
    }

    // ------------------------------------------------------------
    // Shell
    // ------------------------------------------------------------

    public static readonly BindableProperty TitleProperty = BindableProperty.CreateAttached(
        "Title",
        typeof(string),
        typeof(ShellProperty),
        null,
        propertyChanged: PropertyChanged);

    public static string GetTitle(BindableObject bindable) => (string)bindable.GetValue(TitleProperty);

    public static void SetTitle(BindableObject bindable, string value) => bindable.SetValue(TitleProperty, value);

    public static readonly BindableProperty HeaderVisibleProperty = BindableProperty.CreateAttached(
        "HeaderVisible",
        typeof(bool),
        typeof(ShellProperty),
        true,
        propertyChanged: PropertyChanged);

    public static bool GetHeaderVisible(BindableObject bindable) => (bool)bindable.GetValue(HeaderVisibleProperty);

    public static void SetHeaderVisible(BindableObject bindable, bool value) => bindable.SetValue(HeaderVisibleProperty, value);

    public static readonly BindableProperty FunctionVisibleProperty = BindableProperty.CreateAttached(
        "FunctionVisible",
        typeof(bool),
        typeof(ShellProperty),
        true,
        propertyChanged: PropertyChanged);

    public static bool GetFunctionVisible(BindableObject bindable) => (bool)bindable.GetValue(FunctionVisibleProperty);

    public static void SetFunctionVisible(BindableObject bindable, bool value) => bindable.SetValue(FunctionVisibleProperty, value);

    public static readonly BindableProperty Function1TextProperty = BindableProperty.CreateAttached(
        "Function1Text",
        typeof(string),
        typeof(ShellProperty),
        string.Empty,
        propertyChanged: PropertyChanged);

    public static string GetFunction1Text(BindableObject bindable) => (string)bindable.GetValue(Function1TextProperty);

    public static void SetFunction1Text(BindableObject bindable, string value) => bindable.SetValue(Function1TextProperty, value);

    public static readonly BindableProperty Function2TextProperty = BindableProperty.CreateAttached(
        "Function2Text",
        typeof(string),
        typeof(ShellProperty),
        string.Empty,
        propertyChanged: PropertyChanged);

    public static string GetFunction2Text(BindableObject bindable) => (string)bindable.GetValue(Function2TextProperty);

    public static void SetFunction2Text(BindableObject bindable, string value) => bindable.SetValue(Function2TextProperty, value);

    public static readonly BindableProperty Function3TextProperty = BindableProperty.CreateAttached(
        "Function3Text",
        typeof(string),
        typeof(ShellProperty),
        string.Empty,
        propertyChanged: PropertyChanged);

    public static string GetFunction3Text(BindableObject bindable) => (string)bindable.GetValue(Function3TextProperty);

    public static void SetFunction3Text(BindableObject bindable, string value) => bindable.SetValue(Function3TextProperty, value);

    public static readonly BindableProperty Function4TextProperty = BindableProperty.CreateAttached(
        "Function4Text",
        typeof(string),
        typeof(ShellProperty),
        string.Empty,
        propertyChanged: PropertyChanged);

    public static string GetFunction4Text(BindableObject bindable) => (string)bindable.GetValue(Function4TextProperty);

    public static void SetFunction4Text(BindableObject bindable, string value) => bindable.SetValue(Function4TextProperty, value);

    public static readonly BindableProperty Function1EnabledProperty = BindableProperty.CreateAttached(
        "Function1Enabled",
        typeof(bool),
        typeof(ShellProperty),
        false,
        propertyChanged: PropertyChanged);

    public static bool GetFunction1Enabled(BindableObject bindable) => (bool)bindable.GetValue(Function1EnabledProperty);

    public static void SetFunction1Enabled(BindableObject bindable, bool value) => bindable.SetValue(Function1EnabledProperty, value);

    public static readonly BindableProperty Function2EnabledProperty = BindableProperty.CreateAttached(
        "Function2Enabled",
        typeof(bool),
        typeof(ShellProperty),
        false,
        propertyChanged: PropertyChanged);

    public static bool GetFunction2Enabled(BindableObject bindable) => (bool)bindable.GetValue(Function2EnabledProperty);

    public static void SetFunction2Enabled(BindableObject bindable, bool value) => bindable.SetValue(Function2EnabledProperty, value);

    public static readonly BindableProperty Function3EnabledProperty = BindableProperty.CreateAttached(
        "Function3Enabled",
        typeof(bool),
        typeof(ShellProperty),
        false,
        propertyChanged: PropertyChanged);

    public static bool GetFunction3Enabled(BindableObject bindable) => (bool)bindable.GetValue(Function3EnabledProperty);

    public static void SetFunction3Enabled(BindableObject bindable, bool value) => bindable.SetValue(Function3EnabledProperty, value);

    public static readonly BindableProperty Function4EnabledProperty = BindableProperty.CreateAttached(
        "Function4Enabled",
        typeof(bool),
        typeof(ShellProperty),
        false,
        propertyChanged: PropertyChanged);

    public static bool GetFunction4Enabled(BindableObject bindable) => (bool)bindable.GetValue(Function4EnabledProperty);

    public static void SetFunction4Enabled(BindableObject bindable, bool value) => bindable.SetValue(Function4EnabledProperty, value);

    private static void PropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // 現在のビュー以外 (退場中のビュー等) からの変更は反映しない
        if ((currentView is null) || !currentView.TryGetTarget(out var current) || !ReferenceEquals(current, bindable))
        {
            return;
        }

        var parent = ((ContentView)bindable).Parent;
        if (parent?.BindingContext is IShellControl shell)
        {
            UpdateShellControl(shell, bindable);
        }
    }

    public static void UpdateShellControl(IShellControl shell, BindableObject? bindable)
    {
        if (bindable is null)
        {
            shell.Title.Value = string.Empty;
            shell.HeaderVisible.Value = true;
            shell.FunctionVisible.Value = false;
            foreach (var function in shell.Functions)
            {
                function.Text.Value = string.Empty;
                function.Enabled.Value = false;
            }
        }
        else
        {
            shell.Title.Value = GetTitle(bindable);
            shell.HeaderVisible.Value = GetHeaderVisible(bindable);
            shell.FunctionVisible.Value = GetFunctionVisible(bindable);

            var functions = shell.Functions;
            functions[0].Text.Value = GetFunction1Text(bindable);
            functions[1].Text.Value = GetFunction2Text(bindable);
            functions[2].Text.Value = GetFunction3Text(bindable);
            functions[3].Text.Value = GetFunction4Text(bindable);
            functions[0].Enabled.Value = GetFunction1Enabled(bindable);
            functions[1].Enabled.Value = GetFunction2Enabled(bindable);
            functions[2].Enabled.Value = GetFunction3Enabled(bindable);
            functions[3].Enabled.Value = GetFunction4Enabled(bindable);
        }
    }

    // ------------------------------------------------------------
    // BusyOverlay
    // ------------------------------------------------------------

    public static readonly BindableProperty BusyOverlayProperty = BindableProperty.CreateAttached(
        "BusyOverlay",
        typeof(bool),
        typeof(ShellProperty),
        false,
        propertyChanged: OnBusyOverlayChanged);

    public static bool GetBusyOverlay(BindableObject obj) =>
        (bool)obj.GetValue(BusyOverlayProperty);

    public static void SetBusyOverlay(BindableObject obj, bool value) =>
        obj.SetValue(BusyOverlayProperty, value);

    private static void OnBusyOverlayChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not Rectangle view)
        {
            return;
        }

        if (oldValue is true)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is BusyOverlayBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is true)
        {
            view.Behaviors.Add(new BusyOverlayBehavior());
        }
    }

    private sealed class BusyOverlayBehavior : BehaviorBase<Rectangle>
    {
        protected override void OnAttachedTo(Rectangle bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.InputTransparent = false;
            bindable.BackgroundColor = Colors.Transparent;
            bindable.ZIndex = 1000;

            bindable.GestureRecognizers.Add(new TapGestureRecognizer());
        }
    }
}
