namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

using Template.MobileApp.Helpers;
using Template.MobileApp.Models.Entry;

public static class EntryBind
{
    public static readonly BindableProperty ModelProperty = BindableProperty.CreateAttached(
        "Model",
        typeof(IEntryController),
        typeof(EntryBind),
        null,
        propertyChanged: BindChanged);

    public static IEntryController GetModel(BindableObject bindable) =>
        (IEntryController)bindable.GetValue(ModelProperty);

    public static void SetModel(BindableObject bindable, IEntryController value) =>
        bindable.SetValue(ModelProperty, value);

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

            var controller = GetModel(bindable);
            bindable.Completed += BindableOnCompleted;
            bindable.TextChanged += BindableOnTextChanged;
            controller.FocusRequested += ControllerOnFocusRequested;
            controller.PropertyChanged += ControllerOnPropertyChanged;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            var controller = GetModel(bindable);
            bindable.Completed -= BindableOnCompleted;
            bindable.TextChanged -= BindableOnTextChanged;
            controller.FocusRequested -= ControllerOnFocusRequested;
            controller.PropertyChanged -= ControllerOnPropertyChanged;

            base.OnDetachingFrom(bindable);
        }

        private void ControllerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var entry = AssociatedObject;
            if (entry is null)
            {
                return;
            }

            if (e.PropertyName == nameof(EntryModel.Text))
            {
                var controller = GetModel(entry);
                updating = true;
                entry.Text = controller.Text;
                updating = false;
            }
            else if (e.PropertyName == nameof(EntryModel.Enable))
            {
                var controller = GetModel(entry);
                entry.IsEnabled = controller.Enable;
            }
        }

        private void ControllerOnFocusRequested(object? sender, EventArgs e)
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
            var controller = GetModel(entry);
            controller.Text = e.NewTextValue;
        }

        private static void BindableOnCompleted(object? sender, EventArgs e)
        {
            var entry = (Entry)sender!;
            var controller = GetModel(entry);
            var ice = new EntryCompleteEvent();
            controller.HandleCompleted(ice);
            if (!ice.HasError)
            {
                ElementHelper.MoveFocusInRoot(entry, true);
            }
        }
    }
}
