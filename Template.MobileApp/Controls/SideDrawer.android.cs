namespace Template.MobileApp.Controls;

using Microsoft.Maui.Platform;

public sealed partial class SideDrawer
{
    // システムジェスチャから除外できるのは画面の端あたり 200dp まで。帯の上下中央をその分だけ除外する
    private const double ExclusionHeight = 200;

    partial void UpdateGestureExclusion()
    {
        if ((edge.Handler?.PlatformView is not Android.Views.View view) || (view.Context is null) || (edge.Height <= 0))
        {
            return;
        }

        var width = (int)view.Context.ToPixels(edge.Width);
        var height = (int)view.Context.ToPixels(edge.Height);
        var band = (int)view.Context.ToPixels(Math.Min(ExclusionHeight, edge.Height));
        var top = (height - band) / 2;
        using var rect = new Android.Graphics.Rect(0, top, width, top + band);
        view.SystemGestureExclusionRects = [rect];
    }
}
