namespace Template.MobileApp.Behaviors;

using Android.Widget;

using Microsoft.Maui.Handlers;

public static partial class LabelOption
{
    public static partial void UseCustomMapper()
    {
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
    }

    public static void UpdateLabelSize(ILabelHandler handler, Label label)
    {
        var max = (int)GetMaxSize(label);
#pragma warning disable CA1416
        handler.PlatformView.SetAutoSizeTextTypeUniformWithConfiguration(1, max, 1, 1);
#pragma warning restore CA1416
        label.MinimumHeightRequest = max;
    }
}
