namespace Template.MobileApp.Shell;

using Microsoft.Maui.Layouts;

public sealed class OverwrapLayout : AbsoluteLayout
{
    public static readonly BindableProperty OverwrapVisibleProperty =
        BindableProperty.Create(
            nameof(OverwrapVisible),
            typeof(bool),
            typeof(OverwrapLayout),
            false,
            propertyChanged: OnOverwrapVisibleChanged);

    public bool OverwrapVisible
    {
        get => (bool)GetValue(OverwrapVisibleProperty);
        set => SetValue(OverwrapVisibleProperty, value);
    }

    private readonly ContentView overwrapView;

    public View Overwrap
    {
        get => overwrapView.Content;
        set
        {
            if (overwrapView.Content != value)
            {
                overwrapView.Content = value;
            }
        }
    }

    public OverwrapLayout()
    {
        overwrapView = new ContentView
        {
            BackgroundColor = Colors.Transparent,
            ZIndex = 1000
        };
        // ref https://github.com/dotnet/maui/issues/10252
        overwrapView.GestureRecognizers.Add(new TapGestureRecognizer());

        var bindable = (BindableObject)overwrapView;
        SetLayoutBounds(bindable, new Rect(0, 0, 1, 1));
        SetLayoutFlags(bindable, AbsoluteLayoutFlags.All);

        Children.Add(overwrapView);

        overwrapView.IsVisible = OverwrapVisible;
    }

    private static void OnOverwrapVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is OverwrapLayout layout)
        {
            layout.overwrapView.IsVisible = (bool)newValue;
            layout.overwrapView.ZIndex = 1000;
        }
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);

        if ((child is IView view) && (view != overwrapView))
        {
            SetLayoutBounds(view, new Rect(0, 0, 1, 1));
            SetLayoutFlags(view, AbsoluteLayoutFlags.All);
        }
    }
}
