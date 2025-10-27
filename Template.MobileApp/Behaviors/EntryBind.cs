namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

using Template.MobileApp.Helpers;
using Template.MobileApp.Messaging;

public static class EntryBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(EntryController),
        typeof(EntryBind),
        null,
        propertyChanged: BindChanged);

    public static EntryController GetController(BindableObject bindable) =>
        (EntryController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, EntryController value) =>
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
        private EntryController? controller;

        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            if (controller is not null)
            {
                controller.FocusRequest += ControllerOnFocusRequest;
            }

            bindable.Completed += BindableOnCompleted;

            bindable.SetBinding(
                Entry.TextProperty,
                new Binding(nameof(EntryController.Text), source: controller));
            bindable.SetBinding(
                VisualElement.IsEnabledProperty,
                new Binding(nameof(EntryController.Enable), source: controller));
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            if (controller is not null)
            {
                controller.FocusRequest -= ControllerOnFocusRequest;
            }

            bindable.Completed -= BindableOnCompleted;

            bindable.RemoveBinding(Entry.TextProperty);
            bindable.RemoveBinding(VisualElement.IsEnabledProperty);

            controller = null;

            base.OnDetachingFrom(bindable);
        }

        private void ControllerOnFocusRequest(object? sender, EventArgs e)
        {
            AssociatedObject?.Focus();
        }

        private void BindableOnCompleted(object? sender, EventArgs e)
        {
            if (controller is null)
            {
                return;
            }

            var entry = (Entry)sender!;
            var ice = new EntryCompleteEvent();
            controller.HandleCompleted(ice);
            if (!ice.Handled)
            {
                ElementHelper.MoveFocusInRoot(entry, true);
            }
        }
    }
}
