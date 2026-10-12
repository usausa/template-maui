namespace Template.MobileApp.Behaviors;

using System.Runtime.CompilerServices;

using Android.Content.Res;
using Android.Graphics.Drawables;

using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

public static partial class TouchOption
{
    private static readonly ConditionalWeakTable<Android.Views.View, ClickListener> Listeners = [];

    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        ViewHandler.ViewMapper.AppendToMapping(RippleProperty.PropertyName, UpdateRipple);
        ViewHandler.ViewMapper.AppendToMapping(RippleColorProperty.PropertyName, UpdateRipple);
        ViewHandler.ViewMapper.AppendToMapping(ClickCommandProperty.PropertyName, UpdateClickCommand);
    }

    // 付けたビューだけを変える (ほかのビューの前景には触れない)
    private static void UpdateRipple(IViewHandler handler, IView view)
    {
        if ((handler.PlatformView is Android.Views.View platformView) &&
            (view is BindableObject bindable) &&
            bindable.IsSet(RippleProperty))
        {
            if (GetRipple(bindable))
            {
                // 波紋は付けたビューの四角の中だけに描く (マスク)
                using var color = ColorStateList.ValueOf(GetRippleColor(bindable).ToPlatform());
                using var mask = new ColorDrawable(Android.Graphics.Color.White);
                platformView.Foreground = new RippleDrawable(color, null, mask);
                platformView.Clickable = true;
            }
            else
            {
                platformView.Foreground = null;
            }
        }
    }

    private static void UpdateClickCommand(IViewHandler handler, IView view)
    {
        if ((handler.PlatformView is Android.Views.View platformView) &&
            (view is BindableObject bindable) &&
            bindable.IsSet(ClickCommandProperty))
        {
            platformView.SetOnClickListener(GetClickCommand(bindable) is not null ? Listeners.GetValue(platformView, _ => new ClickListener(bindable)) : null);
        }
    }

    private sealed class ClickListener : Java.Lang.Object, Android.Views.View.IOnClickListener
    {
        private readonly WeakReference<BindableObject> target;

        public ClickListener(BindableObject bindable)
        {
            target = new WeakReference<BindableObject>(bindable);
        }

        public void OnClick(Android.Views.View? v)
        {
            if (target.TryGetTarget(out var bindable) && (GetClickCommand(bindable) is { } command))
            {
                var parameter = GetClickCommandParameter(bindable);
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }
        }
    }
}
