namespace Template.MobileApp.Behaviors;

public static partial class LazyViewOption
{
    private static partial void RunAfterDraw(ContentView view, Action action)
    {
        view.Dispatcher.Dispatch(action);
    }
}
