namespace Template.MobileApp.Behaviors;

using System.Linq;

using Microsoft.Maui.Controls.Maps;

using Smart.Maui.Interactivity;

public static class MapBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(IMapController),
        typeof(MapBind),
        null,
        propertyChanged: BindChanged);

    public static IMapController? GetController(BindableObject bindable) =>
        (IMapController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, IMapController? value) =>
        bindable.SetValue(ControllerProperty, value);

    private static void BindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not Map view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is MapBindBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new MapBindBehavior());
        }
    }

    private sealed class MapBindBehavior : BehaviorBase<Map>
    {
        private IMapController? controller;

        protected override void OnAttachedTo(Map bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            controller?.Attach(bindable);
        }

        protected override void OnDetachingFrom(Map bindable)
        {
            controller?.Detach();
            controller = null;

            base.OnDetachingFrom(bindable);
        }
    }
}
