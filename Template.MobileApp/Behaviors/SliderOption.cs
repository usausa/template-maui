namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

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

        var behavior = slider.Behaviors.OfType<SliderSeekBehavior>().FirstOrDefault();
        if (newValue is ICommand command)
        {
            if (behavior is null)
            {
                behavior = new SliderSeekBehavior();
                slider.Behaviors.Add(behavior);
            }

            behavior.Command = command;
        }
        else if (behavior is not null)
        {
            slider.Behaviors.Remove(behavior);
        }
    }

    // ------------------------------------------------------------------ Step

    // 値を Minimum からの刻みにそろえる (0 はそろえない)
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

        var behavior = slider.Behaviors.OfType<SliderStepBehavior>().FirstOrDefault();
        if (newValue is double step and > 0d)
        {
            if (behavior is null)
            {
                behavior = new SliderStepBehavior();
                slider.Behaviors.Add(behavior);
            }

            behavior.Step = step;
        }
        else if (behavior is not null)
        {
            slider.Behaviors.Remove(behavior);
        }
    }
}
