namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

using Template.MobileApp.Messaging;

public static class EntryBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(IEntryController),
        typeof(EntryBind),
        null,
        propertyChanged: BindChanged);

    public static IEntryController GetController(BindableObject bindable) =>
        (IEntryController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, IEntryController value) =>
        bindable.SetValue(ControllerProperty, value);

    private static void BindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not Entry entry)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = entry.Behaviors.FirstOrDefault(static x => x is EntryBindBehavior);
            if (behavior is not null)
            {
                entry.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            entry.Behaviors.Add(new EntryBindBehavior());
        }
    }

    private sealed class EntryBindBehavior : BehaviorBase<Entry>
    {
        private IEntryController? controller;

        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            if ((controller is not null) && (AssociatedObject is not null))
            {
                controller.Attach(bindable);

                bindable.SetBinding(
                    Entry.TextProperty,
                    static (IEntryController controller) => controller.Text,
                    source: controller);
                bindable.SetBinding(
                    VisualElement.IsEnabledProperty,
                    static (IEntryController controller) => controller.Enable,
                    source: controller);
            }
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.RemoveBinding(Entry.TextProperty);
            bindable.RemoveBinding(VisualElement.IsEnabledProperty);

            controller?.Detach();
            controller = null;

            base.OnDetachingFrom(bindable);
        }
    }
}
