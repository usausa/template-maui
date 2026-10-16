namespace Template.MobileApp.Behaviors;

using System.Runtime.CompilerServices;

using Android.Views;

public static partial class LazyViewOption
{
    private static readonly ConditionalWeakTable<View, PreDrawListener> Listeners = [];

    private static partial void RunAfterDraw(ContentView view, Action action)
    {
        if (view.Handler?.PlatformView is View platformView)
        {
            platformView.ViewTreeObserver?.AddOnPreDrawListener(Listeners.GetValue(platformView, x => new PreDrawListener(x, action)));
        }
    }

    private sealed class PreDrawListener : Java.Lang.Object, ViewTreeObserver.IOnPreDrawListener
    {
        private readonly View view;

        private readonly Action action;

        public PreDrawListener(View view, Action action)
        {
            this.view = view;
            this.action = action;
        }

        public bool OnPreDraw()
        {
            view.ViewTreeObserver?.RemoveOnPreDrawListener(this);
            Listeners.Remove(view);
            view.Post(action);
            return true;
        }
    }
}
