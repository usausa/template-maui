namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

using Template.MobileApp.Helpers;
using Template.MobileApp.Messaging;

public static class EntryBind
{
    public static readonly BindableProperty MessengerProperty = BindableProperty.CreateAttached(
        "Messenger",
        typeof(IEntryMessenger),
        typeof(EntryBind),
        null,
        propertyChanged: BindChanged);

    public static IEntryMessenger GetMessenger(BindableObject bindable) =>
        (IEntryMessenger)bindable.GetValue(MessengerProperty);

    public static void SetMessenger(BindableObject bindable, IEntryMessenger value) =>
        bindable.SetValue(MessengerProperty, value);

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
        private bool updating;

        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);

            var controller = GetMessenger(bindable);
            bindable.Completed += BindableOnCompleted;
            bindable.TextChanged += BindableOnTextChanged;
            controller.FocusRequested += MessengerOnFocusRequested;
            controller.PropertyChanged += MessengerOnPropertyChanged;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            var controller = GetMessenger(bindable);
            bindable.Completed -= BindableOnCompleted;
            bindable.TextChanged -= BindableOnTextChanged;
            controller.FocusRequested -= MessengerOnFocusRequested;
            controller.PropertyChanged -= MessengerOnPropertyChanged;

            base.OnDetachingFrom(bindable);
        }

        private void MessengerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var entry = AssociatedObject;
            if (entry is null)
            {
                return;
            }

            if (e.PropertyName == nameof(EntryMessenger.Text))
            {
                var controller = GetMessenger(entry);
                updating = true;
                entry.Text = controller.Text;
                updating = false;
            }
            else if (e.PropertyName == nameof(EntryMessenger.Enable))
            {
                var controller = GetMessenger(entry);
                entry.IsEnabled = controller.Enable;
            }
        }

        private void MessengerOnFocusRequested(object? sender, EventArgs e)
        {
            AssociatedObject?.Focus();
        }

        private void BindableOnTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (updating)
            {
                return;
            }

            var entry = (Entry)sender!;
            var controller = GetMessenger(entry);
            controller.Text = e.NewTextValue;
        }

        private static void BindableOnCompleted(object? sender, EventArgs e)
        {
            var entry = (Entry)sender!;
            var controller = GetMessenger(entry);
            var ice = new EntryCompleteEvent();
            controller.HandleCompleted(ice);
            if (!ice.HasError)
            {
                ElementHelper.MoveFocusInRoot(entry, true);
            }
        }
    }
}
