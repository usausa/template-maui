namespace Template.MobileApp.Behaviors;

#if ANDROID
using Android.Graphics.Drawables;
#endif

using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

public static class Border
{
    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty BorderWidthProperty =
        BindableProperty.CreateAttached(
            "BorderWidth",
            typeof(double),
            typeof(Border),
            default(double));

    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.CreateAttached(
            "BorderColor",
            typeof(Color),
            typeof(Border),
            Colors.Transparent);
    // ReSharper restore InconsistentNaming

    public static void SetBorderWidth(BindableObject bindable, double value) => bindable.SetValue(BorderWidthProperty, value);

    public static double GetBorderWidth(BindableObject bindable) => (double)bindable.GetValue(BorderWidthProperty);

    public static void SetBorderColor(BindableObject bindable, Color value) => bindable.SetValue(BorderColorProperty, value);

    public static Color GetBorderColor(BindableObject bindable) => (Color)bindable.GetValue(BorderColorProperty);

    public static void UseCustomMapper()
    {
#if ANDROID
        EntryHandler.Mapper.Add("BorderWidth", static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));
        EntryHandler.Mapper.Add("BorderColor", static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));

        EditorHandler.Mapper.Add("BorderWidth", static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));
        EditorHandler.Mapper.Add("BorderColor", static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));

        LabelHandler.Mapper.Add("BorderWidth", static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
        LabelHandler.Mapper.Add("BorderColor", static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
#endif
    }

#if ANDROID
    private static void UpdateBehaviors(VisualElement element)
    {
        var width = GetBorderWidth(element);
        var on = width > 0;
        var behavior = element.Behaviors.OfType<BorderBehavior>().FirstOrDefault();
        if (on)
        {
            if (behavior is not null)
            {
                behavior.UpdateBorder();
            }
            else
            {
                element.Behaviors.Add(new BorderBehavior());
            }
        }
        else if (behavior is not null)
        {
            element.Behaviors.Remove(behavior);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Ignore")]
    private sealed class BorderBehavior : PlatformBehavior<VisualElement, Android.Views.View>
    {
        private Drawable? originalDrawable;

        private GradientDrawable drawable = default!;

        private VisualElement? element;

        private Android.Views.View view = default!;

        protected override void OnAttachedTo(VisualElement bindable, Android.Views.View platformView)
        {
            base.OnAttachedTo(bindable, platformView);

            element = bindable;
            view = platformView;

            originalDrawable = platformView.Background;
            drawable = new GradientDrawable();
            platformView.Background = drawable;

            UpdateBorder();
        }

        protected override void OnDetachedFrom(VisualElement bindable, Android.Views.View platformView)
        {
            platformView.Background = originalDrawable;
            drawable.Dispose();

            element = null;
            view = null!;

            base.OnDetachedFrom(bindable, platformView);
        }

        internal void UpdateBorder()
        {
            if (element is null)
            {
                return;
            }

            var width = (int)view.Context.ToPixels(GetBorderWidth(element));
            var color = GetBorderColor(element).ToAndroid();
            drawable.SetStroke(width, color);
            drawable.SetColor(element.BackgroundColor.ToAndroid());
        }
    }
#endif
}
