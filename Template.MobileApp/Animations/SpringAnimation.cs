namespace Template.MobileApp.Animations;

using Smart.Maui.Animations;

public sealed class SpringAnimation : AnimationBase
{
    protected override async Task BeginAnimation(VisualElement target)
    {
        await Task.WhenAll(
            target.TranslateToAsync(150, 0, 500, Easing.SinOut),
            target.ScaleToAsync(0.85, 300, Easing.CubicOut));

        await target.ScaleToAsync(1.05, 250, Easing.CubicIn);

        await target.ScaleToAsync(1.0, 200);
        await target.TranslateToAsync(0, 0, 400, Easing.SpringOut);
    }
}
