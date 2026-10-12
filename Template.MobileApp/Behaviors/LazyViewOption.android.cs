namespace Template.MobileApp.Behaviors;

using System.Runtime.CompilerServices;

using Android.Views;

public static partial class LazyViewOption
{
    private static readonly ConditionalWeakTable<Android.Views.View, PreDrawListener> Listeners = [];

    private static partial void RunAfterDraw(ContentView view, Action action)
    {
        if (view.Handler?.PlatformView is Android.Views.View platformView)
        {
            platformView.ViewTreeObserver?.AddOnPreDrawListener(Listeners.GetValue(platformView, x => new PreDrawListener(x, action)));
        }
    }

    // 描画の直前に外し、描画が終わった後に実行する
    private sealed class PreDrawListener : Java.Lang.Object, ViewTreeObserver.IOnPreDrawListener
    {
        private readonly Android.Views.View view;

        private readonly Action action;

        public PreDrawListener(Android.Views.View view, Action action)
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
