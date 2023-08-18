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
    public static readonly BindableProperty WidthProperty =
        BindableProperty.CreateAttached(
            "Width",
            typeof(double?),
            typeof(Border),
            default(double?));

    public static readonly BindableProperty ColorProperty =
        BindableProperty.CreateAttached(
            "Color",
            typeof(Color),
            typeof(Border),
            Colors.Transparent);

    public static readonly BindableProperty PaddingProperty =
        BindableProperty.CreateAttached(
            "Padding",
            typeof(Thickness),
            typeof(Border),
            default(Thickness));

    public static readonly BindableProperty RadiusProperty =
        BindableProperty.CreateAttached(
            "Radius",
            typeof(double?),
            typeof(Border),
            default(double?));
    // ReSharper restore InconsistentNaming

    public static void SetWidth(BindableObject bindable, double? value) => bindable.SetValue(WidthProperty, value);

    public static double? GetWidth(BindableObject bindable) => (double?)bindable.GetValue(WidthProperty);

    public static void SetColor(BindableObject bindable, Color value) => bindable.SetValue(ColorProperty, value);

    public static Color GetColor(BindableObject bindable) => (Color)bindable.GetValue(ColorProperty);

    public static void SetPadding(BindableObject bindable, Thickness value) => bindable.SetValue(PaddingProperty, value);

    public static Thickness GetPadding(BindableObject bindable) => (Thickness)bindable.GetValue(PaddingProperty);

    public static void SetRadius(BindableObject bindable, double? value) => bindable.SetValue(RadiusProperty, value);

    public static double? GetRadius(BindableObject bindable) => (double?)bindable.GetValue(RadiusProperty);

    public static void UseCustomMapper(BehaviorOptions options)
    {
#if ANDROID
        if (options.Border)
        {
            EntryHandler.Mapper.Add(WidthProperty.PropertyName, static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));
            EntryHandler.Mapper.Add(ColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));
            EntryHandler.Mapper.Add(PaddingProperty.PropertyName, static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));
            EntryHandler.Mapper.Add(RadiusProperty.PropertyName, static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));
            EntryHandler.Mapper.Add(VisualElement.BackgroundColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));

            EditorHandler.Mapper.Add(WidthProperty.PropertyName, static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));
            EditorHandler.Mapper.Add(ColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));
            EditorHandler.Mapper.Add(PaddingProperty.PropertyName, static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));
            EditorHandler.Mapper.Add(RadiusProperty.PropertyName, static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));
            EditorHandler.Mapper.Add(VisualElement.BackgroundColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));

            LabelHandler.Mapper.Add(WidthProperty.PropertyName, static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
            LabelHandler.Mapper.Add(ColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
            LabelHandler.Mapper.Add(PaddingProperty.PropertyName, static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
            LabelHandler.Mapper.Add(RadiusProperty.PropertyName, static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
            LabelHandler.Mapper.Add(VisualElement.BackgroundColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
        }
#endif
    }

#if ANDROID
    private static void UpdateBehaviors(VisualElement element)
    {
        var width = GetWidth(element);
        var padding = GetPadding(element);
        var radius = GetRadius(element);

        var behavior = element.Behaviors.OfType<BorderBehavior>().FirstOrDefault();
        if (width.HasValue || (padding != Thickness.Zero) || radius.HasValue)
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

            var width = GetWidth(element);
            if (width.HasValue)
            {
                var strokeWidth = (int)view.Context.ToPixels(width.Value);
                var color = GetColor(element).ToAndroid();
                drawable.SetStroke(strokeWidth, color);
            }

            var radius = GetRadius(element);
            if (radius.HasValue)
            {
                var cornerRadius = (int)view.Context.ToPixels(radius.Value);
                drawable.SetCornerRadius(cornerRadius);
            }

            if (element.BackgroundColor is not null)
            {
                drawable.SetColor(element.BackgroundColor.ToAndroid());
            }

            var padding = GetPadding(element);
            if (padding != Thickness.Zero)
            {
                var paddingLeft = (int)view.Context.ToPixels(padding.Left);
                var paddingTop = (int)view.Context.ToPixels(padding.Top);
                var paddingRight = (int)view.Context.ToPixels(padding.Right);
                var paddingBottom = (int)view.Context.ToPixels(padding.Bottom);
                view.SetPadding(paddingLeft, paddingTop, paddingRight, paddingBottom);
            }

            view.ClipToOutline = true;
        }
    }
#endif
}
