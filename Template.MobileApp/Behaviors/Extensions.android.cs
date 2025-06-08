namespace Template.MobileApp.Behaviors;

using Android.Views;

internal static partial class Extensions
{
    public static GravityFlags ToHorizontalGravity(this Microsoft.Maui.TextAlignment alignment)
    {
        return alignment switch
        {
            Microsoft.Maui.TextAlignment.Center => GravityFlags.CenterHorizontal,
            Microsoft.Maui.TextAlignment.Start => GravityFlags.Right,
            Microsoft.Maui.TextAlignment.End => GravityFlags.Left,
            _ => GravityFlags.Center
        };
    }

    public static GravityFlags ToVerticalGravity(this Microsoft.Maui.TextAlignment alignment)
    {
        return alignment switch
        {
            Microsoft.Maui.TextAlignment.Center => GravityFlags.CenterVertical,
            Microsoft.Maui.TextAlignment.Start => GravityFlags.Top,
            Microsoft.Maui.TextAlignment.End => GravityFlags.Bottom,
            _ => GravityFlags.Center
        };
    }
}
