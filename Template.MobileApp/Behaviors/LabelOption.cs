namespace Template.MobileApp.Behaviors;

using Android.Widget;

using Microsoft.Maui.Handlers;

public static class LabelOption
{
    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty AutoSizeProperty = BindableProperty.CreateAttached(
        "AutoSize",
        typeof(bool),
        typeof(LabelOption),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetAutoSize(BindableObject bindable) => (bool)bindable.GetValue(AutoSizeProperty);

    public static void SetAutoSize(BindableObject bindable, bool value) => bindable.SetValue(AutoSizeProperty, value);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty MaxSizeProperty = BindableProperty.CreateAttached(
        "MaxSize",
        typeof(double),
        typeof(LabelOption),
        144d);
    // ReSharper restore InconsistentNaming

    public static double GetMaxSize(BindableObject bindable) => (double)bindable.GetValue(MaxSizeProperty);

    public static void SetMaxSize(BindableObject bindable, double value) => bindable.SetValue(MaxSizeProperty, value);

    public static void UseCustomMapper()
    {
#if ANDROID
        LabelHandler.Mapper.AppendToMapping("AutoSize", (handler, view) =>
        {
            var label = (Label)view;
            if (GetAutoSize(label))
            {
                label.LineBreakMode = LineBreakMode.NoWrap;
#pragma warning disable CA1416
                handler.PlatformView.SetAutoSizeTextTypeWithDefaults(AutoSizeTextType.Uniform);
#pragma warning restore CA1416

                UpdateLabelSize(handler, label);
            }
        });
        LabelHandler.Mapper.AppendToMapping("MaxSize", (handler, view) =>
        {
            var label = (Label)view;
            if (GetAutoSize(label))
            {
                UpdateLabelSize(handler, label);
            }
        });
#endif
    }

#if ANDROID
    public static void UpdateLabelSize(ILabelHandler handler, Label label)
    {
        var max = (int)GetMaxSize(label);
#pragma warning disable CA1416
        handler.PlatformView.SetAutoSizeTextTypeUniformWithConfiguration(1, max, 1, 1);
#pragma warning restore CA1416
        label.MinimumHeightRequest = max;
    }
#endif
}
