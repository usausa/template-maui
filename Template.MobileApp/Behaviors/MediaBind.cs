namespace Template.MobileApp.Behaviors;

using System.Linq;

using CommunityToolkit.Maui.Views;

using Smart.Maui.Interactivity;

public static class MediaBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(IMediaController),
        typeof(MediaBind),
        null,
        propertyChanged: BindChanged);

    public static IMediaController? GetController(BindableObject bindable) =>
        (IMediaController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, IMediaController? value) =>
        bindable.SetValue(ControllerProperty, value);

    private static void BindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not MediaElement view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is MediaBindBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new MediaBindBehavior());
        }
    }

    private sealed class MediaBindBehavior : BehaviorBase<MediaElement>
    {
        private IMediaController? controller;

        protected override void OnAttachedTo(MediaElement bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            controller?.Attach(bindable);
        }

        protected override void OnDetachingFrom(MediaElement bindable)
        {
            controller?.Detach();
            controller = null;

            base.OnDetachingFrom(bindable);
        }
    }
}
