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
}
