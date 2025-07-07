namespace Template.MobileApp.Behaviors;

using Android.Graphics.Drawables;

using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

public static partial class Border
{
    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        if (options.Border)
        {
            EntryHandler.Mapper.AppendToMapping(WidthProperty.PropertyName, static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));
            EntryHandler.Mapper.AppendToMapping(ColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));
            EntryHandler.Mapper.AppendToMapping(PaddingProperty.PropertyName, static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));
            EntryHandler.Mapper.AppendToMapping(RadiusProperty.PropertyName, static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));
            EntryHandler.Mapper.AppendToMapping(VisualElement.BackgroundColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Entry)handler.VirtualView));

            EditorHandler.Mapper.AppendToMapping(WidthProperty.PropertyName, static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));
            EditorHandler.Mapper.AppendToMapping(ColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));
            EditorHandler.Mapper.AppendToMapping(PaddingProperty.PropertyName, static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));
            EditorHandler.Mapper.AppendToMapping(RadiusProperty.PropertyName, static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));
            EditorHandler.Mapper.AppendToMapping(VisualElement.BackgroundColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Editor)handler.VirtualView));

            LabelHandler.Mapper.AppendToMapping(WidthProperty.PropertyName, static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
            LabelHandler.Mapper.AppendToMapping(ColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
            LabelHandler.Mapper.AppendToMapping(PaddingProperty.PropertyName, static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
            LabelHandler.Mapper.AppendToMapping(RadiusProperty.PropertyName, static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
            LabelHandler.Mapper.AppendToMapping(VisualElement.BackgroundColorProperty.PropertyName, static (handler, _) => UpdateBehaviors((Label)handler.VirtualView));
        }
    }

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

#pragma warning disable CA1001
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
                var color = GetColor(element).ToPlatform();
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
                drawable.SetColor(element.BackgroundColor.ToPlatform());
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
#pragma warning restore CA1001
}
