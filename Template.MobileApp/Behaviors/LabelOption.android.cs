namespace Template.MobileApp.Behaviors;

using Android.Util;
using Android.Widget;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

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

        if (options.FixedLineHeight)
        {
            LabelHandler.Mapper.AppendToMapping(FixedLineHeightProperty.PropertyName, static (handler, view) =>
            {
                var label = (Label)view;
                var height = GetFixedLineHeight(label);
                if (height > 0)
                {
                    var textView = handler.PlatformView;
                    // MAUI does not set the label font to the TextView for FormattedText
                    if (label.FormattedText is not null)
                    {
                        textView.UpdateFont(label, handler.MauiContext!.Services.GetRequiredService<IFontManager>());
                    }

                    // CJK fallback fonts expand the line
                    textView.FallbackLineSpacing = false;
                    textView.SetIncludeFontPadding(false);
                    textView.LineHeight = (int)Math.Round(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)height, textView.Resources!.DisplayMetrics));
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
