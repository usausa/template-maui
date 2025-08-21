namespace Template.MobileApp.Animations;

using Smart.Maui.Animations;

public sealed class SpringAnimation : AnimationBase
{
    protected override async Task BeginAnimation(VisualElement target)
    {
        await Task.WhenAll(
            target.TranslateTo(150, 0, 500, Easing.SinOut),
            target.ScaleTo(0.85, 300, Easing.CubicOut));

        await target.ScaleTo(1.05, 250, Easing.CubicIn);

        await target.ScaleTo(1.0, 200);
        await target.TranslateTo(0, 0, 400, Easing.SpringOut);
    }
}
