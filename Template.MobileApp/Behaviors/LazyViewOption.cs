namespace Template.MobileApp.Behaviors;

using CommunityToolkit.Maui.Views;

public static partial class LazyViewOption
{
    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty LoadProperty = BindableProperty.CreateAttached(
        "Load",
        typeof(bool),
        typeof(LazyViewOption),
        false,
        propertyChanged: HandleLoadChanged);

    public static bool GetLoad(BindableObject bindable) => (bool)bindable.GetValue(LoadProperty);

    public static void SetLoad(BindableObject bindable, bool value) => bindable.SetValue(LoadProperty, value);

    public static readonly BindableProperty TemplateProperty = BindableProperty.CreateAttached(
        "Template",
        typeof(DataTemplate),
        typeof(LazyViewOption),
        null,
        propertyChanged: HandleTemplateChanged);

    public static DataTemplate? GetTemplate(BindableObject bindable) => (DataTemplate?)bindable.GetValue(TemplateProperty);

    public static void SetTemplate(BindableObject bindable, DataTemplate? value) => bindable.SetValue(TemplateProperty, value);

    public static readonly BindableProperty DeferredProperty = BindableProperty.CreateAttached(
        "Deferred",
        typeof(bool),
        typeof(LazyViewOption),
        false,
        propertyChanged: HandleDeferredChanged);

    public static bool GetDeferred(BindableObject bindable) => (bool)bindable.GetValue(DeferredProperty);

    public static void SetDeferred(BindableObject bindable, bool value) => bindable.SetValue(DeferredProperty, value);

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

    private static void HandleLoadChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if ((bindable is LazyView view) && (newValue is true) && !view.HasLazyViewLoaded)
        {
            _ = LoadAsync(view);
        }
        else if ((bindable is ContentView content) && (newValue is true))
        {
            LoadTemplate(content);
        }
    }

    private static void HandleTemplateChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if ((bindable is ContentView content) && GetLoad(content))
        {
            LoadTemplate(content);
        }
    }

    private static void HandleDeferredChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if ((bindable is ContentView view) && (newValue is true))
        {
            view.HandlerChanged += HandleDeferredHandlerChanged;
        }
    }

    private static void HandleDeferredHandlerChanged(object? sender, EventArgs e)
    {
        if ((sender is ContentView view) && (view.Handler is not null))
        {
            view.HandlerChanged -= HandleDeferredHandlerChanged;
            RunAfterDraw(view, () => SetLoad(view, true));
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static partial void RunAfterDraw(ContentView view, Action action);

    private static void LoadTemplate(ContentView view)
    {
        if ((view.Content is null) && (GetTemplate(view) is { } template))
        {
            view.Content = (View)template.CreateContent();
        }
    }

    private static async Task LoadAsync(LazyView view) => await view.LoadViewAsync().ConfigureAwait(true);
}
