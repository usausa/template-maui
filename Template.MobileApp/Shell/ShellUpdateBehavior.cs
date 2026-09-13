namespace Template.MobileApp.Shell;

using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

using Smart.Maui.Interactivity;

public sealed class ShellUpdateBehavior : BehaviorBase<ContentPage>
{
    public static readonly BindableProperty NavigatorProperty = BindableProperty.Create(
        nameof(Navigator),
        typeof(INavigator),
        typeof(ShellUpdateBehavior),
        propertyChanged: HandlePropertyChanged);

    public INavigator? Navigator
    {
        get => (INavigator)GetValue(NavigatorProperty);
        set => SetValue(NavigatorProperty, value);
    }
    // ステータスバーの変更を購読している IShellControl
    private IShellControl? shell;

    private StatusBarBehavior? statusBar;
    private Color defaultStatusBarColor = Colors.Transparent;
    private StatusBarStyle defaultStatusBarStyle = StatusBarStyle.Default;

    protected override void OnDetachingFrom(ContentPage bindable)
    {
        if (Navigator is not null)
        {
            Navigator.Navigating -= NavigatorOnNavigating;
            Navigator.Exited -= NavigatorOnExited;
        }

        DetachShell();

        base.OnDetachingFrom(bindable);
    }

    private static void HandlePropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        ((ShellUpdateBehavior)bindable).OnNavigatorPropertyChanged(oldValue as INavigator, newValue as INavigator);
    }

    private void OnNavigatorPropertyChanged(INavigator? oldValue, INavigator? newValue)
    {
        if (newValue == oldValue)
        {
            return;
        }

        if (oldValue is not null)
        {
            oldValue.Navigating -= NavigatorOnNavigating;
            oldValue.Exited -= NavigatorOnExited;
        }

        if (newValue is not null)
        {
            newValue.Navigating += NavigatorOnNavigating;
            newValue.Exited += NavigatorOnExited;
        }
    }

    private void NavigatorOnNavigating(object? sender, Smart.Navigation.NavigationEventArgs e)
    {
        UpdateShell(e.ToView as Element);
    }

    private void NavigatorOnExited(object? sender, EventArgs e)
    {
        UpdateShell(null);
    }

    private void UpdateShell(BindableObject? view)
    {
        ShellProperty.SetCurrentView(view);

        if (AssociatedObject?.BindingContext is IShellControl control)
        {
            AttachShell(control);
            ShellProperty.UpdateShellControl(control, view);
        }
    }

    private void AttachShell(IShellControl control)
    {
        if (ReferenceEquals(shell, control))
        {
            return;
        }

        DetachShell();

        shell = control;
        control.StatusBarColor.PropertyChanged += OnStatusBarChanged;
        control.StatusBarStyle.PropertyChanged += OnStatusBarChanged;
    }

    private void DetachShell()
    {
        if (shell is null)
        {
            return;
        }

        shell.StatusBarColor.PropertyChanged -= OnStatusBarChanged;
        shell.StatusBarStyle.PropertyChanged -= OnStatusBarChanged;
        shell = null;
    }

    private void OnStatusBarChanged(object? sender, PropertyChangedEventArgs e)
    {
        ApplyStatusBar();
    }

    private void ApplyStatusBar()
    {
        if (shell is null)
        {
            return;
        }

        if (statusBar is null)
        {
            statusBar = AssociatedObject?.Behaviors.OfType<StatusBarBehavior>().FirstOrDefault();
            if (statusBar is null)
            {
                return;
            }

            defaultStatusBarColor = statusBar.StatusBarColor;
            defaultStatusBarStyle = statusBar.StatusBarStyle;
        }

        var style = shell.StatusBarStyle.Value;
        statusBar.StatusBarColor = shell.StatusBarColor.Value ?? defaultStatusBarColor;
        statusBar.StatusBarStyle = style == StatusBarStyle.Default ? defaultStatusBarStyle : style;
    }
}
