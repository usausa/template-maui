namespace Template.MobileApp.Behaviors;

using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Widget;

using Google.Android.Material.Button;

using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

using AColor = Android.Graphics.Color;

public static partial class StepperOption
{
    private static readonly AColor OutlinedBackgroundColor = AColor.White;
    private static readonly AColor OutlinedStrokeColor = AColor.ParseColor("#BDBDBD");
    private static readonly int OutlinedTextColor = AColor.ParseColor("#455A64").ToArgb();
    private static readonly int OutlinedDisabledTextColor = AColor.ParseColor("#BDBDBD").ToArgb();

    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        StepperHandler.Mapper.AppendToMapping(OutlinedProperty.PropertyName, static (handler, _) => UpdateOutlined(handler.PlatformView, (Stepper)handler.VirtualView));
    }

    // -/+ のボタンを幅 48・高さ 32 の白地・灰の枠・角丸 8 にして、間を 8 空ける
    private static void UpdateOutlined(LinearLayout stepper, BindableObject element)
    {
        if (!GetOutlined(element))
        {
            return;
        }

        var context = stepper.Context;
        var radius = (int)context.ToPixels(8);
        var stroke = (int)context.ToPixels(1);
        var space = (int)context.ToPixels(8);
        var width = (int)context.ToPixels(48);
        var height = (int)context.ToPixels(32);
        using var textColors = new ColorStateList(
            [[-global::Android.Resource.Attribute.StateEnabled], []],
            [OutlinedDisabledTextColor, OutlinedTextColor]);

        for (var i = 0; i < stepper.ChildCount; i++)
        {
            if (stepper.GetChildAt(i) is not Android.Widget.Button button)
            {
                continue;
            }

            if (button is MaterialButton materialButton)
            {
                materialButton.BackgroundTintList = ColorStateList.ValueOf(OutlinedBackgroundColor);
                materialButton.StrokeColor = ColorStateList.ValueOf(OutlinedStrokeColor);
                materialButton.StrokeWidth = stroke;
                materialButton.CornerRadius = radius;
                materialButton.InsetTop = 0;
                materialButton.InsetBottom = 0;
            }
            else
            {
                using var background = new GradientDrawable();
                background.SetColor(OutlinedBackgroundColor);
                background.SetStroke(stroke, OutlinedStrokeColor);
                background.SetCornerRadius(radius);
                button.Background = background;
            }

            button.SetTextColor(textColors);
            button.Elevation = 0;
            button.StateListAnimator = null;
            button.SetMinWidth(0);
            button.SetMinimumWidth(0);
            button.SetMinHeight(0);
            button.SetMinimumHeight(0);
            button.SetPadding(0, 0, 0, 0);

            if (button.LayoutParameters is LinearLayout.LayoutParams layoutParameters)
            {
                layoutParameters.Width = width;
                layoutParameters.Height = height;
                layoutParameters.LeftMargin = i > 0 ? space : 0;
                button.LayoutParameters = layoutParameters;
            }
        }
    }
}
