namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

using SmartBehavior = Smart.Maui.Behavior;

public static class ValidationOption
{
    private const string ItemsPropertyName = "Item[]";

    public static readonly BindableProperty ErrorMessageProperty = BindableProperty.CreateAttached(
        "ErrorMessage",
        typeof(string),
        typeof(ValidationOption),
        null,
        propertyChanged: OnErrorMessageChanged);

    public static string? GetErrorMessage(BindableObject bindable) => (string?)bindable.GetValue(ErrorMessageProperty);

    public static void SetErrorMessage(BindableObject bindable, string? value) => bindable.SetValue(ErrorMessageProperty, value);

    public static readonly BindableProperty ErrorHighlightProperty = BindableProperty.CreateAttached(
        "ErrorHighlight",
        typeof(string),
        typeof(ValidationOption),
        null,
        propertyChanged: OnErrorHighlightChanged);

    public static string? GetErrorHighlight(BindableObject bindable) => (string?)bindable.GetValue(ErrorHighlightProperty);

    public static void SetErrorHighlight(BindableObject bindable, string? value) => bindable.SetValue(ErrorHighlightProperty, value);

    public static readonly BindableProperty ErrorFieldProperty = BindableProperty.CreateAttached(
        "ErrorField",
        typeof(string),
        typeof(ValidationOption),
        null,
        propertyChanged: OnErrorFieldChanged);

    public static string? GetErrorField(BindableObject bindable) => (string?)bindable.GetValue(ErrorFieldProperty);

    public static void SetErrorField(BindableObject bindable, string? value) => bindable.SetValue(ErrorFieldProperty, value);

    private static void OnErrorMessageChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is Label label)
        {
            Replace(label, newValue as string, static key => new ErrorMessageBehavior { Key = key });
        }
    }

    private static void OnErrorHighlightChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is VisualElement element)
        {
            Replace(element, newValue as string, static key => new ErrorHighlightBehavior { Key = key });
        }
    }

    private static void OnErrorFieldChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is VisualElement element)
        {
            Replace(element, newValue as string, static key => new ErrorFieldBehavior { Key = key });
        }
    }

    private static void Replace<TBehavior>(VisualElement element, string? key, Func<string, TBehavior> factory)
        where TBehavior : ErrorBehavior
    {
        var behavior = element.Behaviors.OfType<TBehavior>().FirstOrDefault();
        if (behavior is not null)
        {
            element.Behaviors.Remove(behavior);
        }

        if (!String.IsNullOrEmpty(key))
        {
            element.Behaviors.Add(factory(key));
        }
    }

    private abstract class ErrorBehavior : BehaviorBase<VisualElement>
    {
        private readonly Action applyChanged;

        private ErrorInfo? errors;

        private string? current;

        private bool pending;

        protected ErrorBehavior()
        {
            applyChanged = ApplyChanged;
        }

        public string Key { get; init; } = string.Empty;

        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);

            if (errors is null)
            {
                Apply(true);
            }
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            Subscribe(null);

            base.OnDetachingFrom(bindable);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (AssociatedObject is null)
            {
                return;
            }

            var next = (BindingContext as ViewModelBase)?.Errors;
            if (ReferenceEquals(next, errors))
            {
                return;
            }

            Subscribe(next);
            Apply(true);
        }

        protected abstract void Update(VisualElement element, string? previous, string? value, bool initial);

        private void Subscribe(ErrorInfo? next)
        {
            if (errors is not null)
            {
                errors.PropertyChanged -= OnErrorsPropertyChanged;
            }

            errors = next;

            if (errors is not null)
            {
                errors.PropertyChanged += OnErrorsPropertyChanged;
            }
        }

        private void OnErrorsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.PropertyName) && (e.PropertyName != ItemsPropertyName))
            {
                return;
            }

            if (pending || (AssociatedObject is not { } element))
            {
                return;
            }

            if (!element.Dispatcher.IsDispatchRequired && (Read() == current))
            {
                return;
            }

            pending = true;
            element.Dispatcher.Dispatch(applyChanged);
        }

        private void ApplyChanged()
        {
            pending = false;
            Apply(false);
        }

        private string? Read()
        {
            var value = errors?[Key];
            return String.IsNullOrEmpty(value) ? null : value;
        }

        private void Apply(bool initial)
        {
            if (AssociatedObject is not { } element)
            {
                return;
            }

            var value = Read();
            if (!initial && (value == current))
            {
                return;
            }

            var previous = current;
            current = value;
            Update(element, previous, value, initial);
        }
    }

    private sealed class ErrorMessageBehavior : ErrorBehavior
    {
        protected override void Update(VisualElement element, string? previous, string? value, bool initial)
        {
            if (element is not Label label)
            {
                return;
            }

            label.Text = value;
            label.IsVisible = value is not null;

            if (!initial && (value is not null))
            {
                AnimationOption.PlayFadeIn(label);
            }
        }
    }

    private sealed class ErrorHighlightBehavior : ErrorBehavior
    {
        protected override void Update(VisualElement element, string? previous, string? value, bool initial)
        {
            if (!initial && (previous is not null) && (value is not null))
            {
                AnimationOption.PlayHighlight(element);
            }
        }
    }

    private sealed class ErrorFieldBehavior : ErrorBehavior
    {
        private bool keyAssigned;

        protected override void OnAttachedTo(VisualElement bindable)
        {
            if (String.IsNullOrEmpty(SmartBehavior.GetKey(bindable)))
            {
                SmartBehavior.SetKey(bindable, Key);
                keyAssigned = true;
            }

            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            base.OnDetachingFrom(bindable);

            if (keyAssigned)
            {
                SmartBehavior.SetKey(bindable, null);
                keyAssigned = false;
            }
        }

        protected override void Update(VisualElement element, string? previous, string? value, bool initial)
        {
            Focus.SetHasError(element, value is not null);
        }
    }
}
