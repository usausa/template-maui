namespace Template.MobileApp.Behaviors;

public static class SliderOption
{
    // ------------------------------------------------------------------ DragCompletedCommand

    public static readonly BindableProperty DragCompletedCommandProperty = BindableProperty.CreateAttached(
        "DragCompletedCommand",
        typeof(ICommand),
        typeof(SliderOption),
        null,
        propertyChanged: OnDragCompletedCommandChanged);

    public static ICommand? GetDragCompletedCommand(BindableObject bindable) => (ICommand?)bindable.GetValue(DragCompletedCommandProperty);

    public static void SetDragCompletedCommand(BindableObject bindable, ICommand? value) => bindable.SetValue(DragCompletedCommandProperty, value);

    private static void OnDragCompletedCommandChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not Slider slider)
        {
            return;
        }

        if (oldValue is not null)
        {
            slider.DragCompleted -= OnDragCompleted;
        }
        if (newValue is not null)
        {
            slider.DragCompleted += OnDragCompleted;
        }
    }

    private static void OnDragCompleted(object? sender, EventArgs e)
    {
        if (sender is not Slider slider)
        {
            return;
        }

        var command = GetDragCompletedCommand(slider);
        if (command?.CanExecute(slider.Value) ?? false)
        {
            command.Execute(slider.Value);
        }
    }

    // ------------------------------------------------------------------ Step

    // 値を刻みの倍数にそろえる (0 はそろえない)
    public static readonly BindableProperty StepProperty = BindableProperty.CreateAttached(
        "Step",
        typeof(double),
        typeof(SliderOption),
        0d,
        propertyChanged: OnStepChanged);

    public static double GetStep(BindableObject bindable) => (double)bindable.GetValue(StepProperty);

    public static void SetStep(BindableObject bindable, double value) => bindable.SetValue(StepProperty, value);

    private static void OnStepChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not Slider slider)
        {
            return;
        }

        if (oldValue is > 0d)
        {
            slider.ValueChanged -= OnValueChanged;
        }
        if (newValue is > 0d)
        {
            slider.ValueChanged += OnValueChanged;
        }
    }

    private static void OnValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (sender is not Slider slider)
        {
            return;
        }

        // Setting the same value does not raise the event again
        var step = GetStep(slider);
        slider.Value = Math.Round(e.NewValue / step) * step;
    }
}
