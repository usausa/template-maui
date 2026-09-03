namespace Template.MobileApp.Behaviors;

using Android.Widget;

using Microsoft.Maui.Handlers;

public static partial class LabelOption
{
    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        if (options.AutoSize)
        {
            LabelHandler.Mapper.AppendToMapping(AutoSizeProperty.PropertyName, (handler, view) =>
            {
                var label = (Label)view;
                if (GetAutoSize(label))
                {
                    label.LineBreakMode = LineBreakMode.NoWrap;
                    handler.PlatformView.SetAutoSizeTextTypeWithDefaults(AutoSizeTextType.Uniform);

                    UpdateLabelSize(handler, label);
                }
            });
            LabelHandler.Mapper.AppendToMapping(MaxSizeProperty.PropertyName, (handler, view) =>
            {
                var label = (Label)view;
                if (GetAutoSize(label))
                {
                    UpdateLabelSize(handler, label);
                }
            });
        }
    }

    public static void UpdateLabelSize(ILabelHandler handler, Label label)
    {
        var max = (int)GetMaxSize(label);
        handler.PlatformView.SetAutoSizeTextTypeUniformWithConfiguration(1, max, 1, 1);
        label.MinimumHeightRequest = max;
    }
}
