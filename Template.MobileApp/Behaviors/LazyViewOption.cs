namespace Template.MobileApp.Behaviors;

using CommunityToolkit.Maui.Views;

// CommunityToolkit の LazyView は LoadViewAsync() 呼び出しで初めて中身を生成する。
// code-behind を使わず VM のフラグから起動できるよう添付プロパティで橋渡しする
public static class LazyViewOption
{
    public static readonly BindableProperty LoadProperty = BindableProperty.CreateAttached(
        "Load",
        typeof(bool),
        typeof(LazyViewOption),
        false,
        propertyChanged: HandleLoadChanged);

    public static bool GetLoad(BindableObject bindable) => (bool)bindable.GetValue(LoadProperty);

    public static void SetLoad(BindableObject bindable, bool value) => bindable.SetValue(LoadProperty, value);

    private static void HandleLoadChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if ((bindable is LazyView view) && (newValue is true) && !view.HasLazyViewLoaded)
        {
            _ = LoadAsync(view);
        }
    }

    private static async Task LoadAsync(LazyView view) => await view.LoadViewAsync().ConfigureAwait(true);
}
