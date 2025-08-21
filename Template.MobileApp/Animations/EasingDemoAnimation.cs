namespace Template.MobileApp.Animations;

using Smart.Maui.Animations;

public sealed class EasingDemoAnimation : AnimationBase
{
    protected override async Task BeginAnimation(VisualElement target)
    {
        target.TranslationX = 0;
        target.TranslationY = 0;

        var parent = target.Parent.FindParent<VisualElement>();
        if (parent is null)
        {
            return;
        }

        var x = parent.Width - target.Width;
        var y = parent.Height - target.Height;
        await target.TranslateTo(x, y, Duration, Easing);
    }
}
