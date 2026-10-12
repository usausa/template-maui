namespace Template.MobileApp.Animations;

using Smart.Maui.Animations;

using Template.MobileApp.Controls;

// EasingCurveView の丸を、時間に比例して始点から終点まで動かす(縦の位置は曲線の Easing の値)
public sealed class EasingDemoAnimation : AnimationBase
{
    protected override Task BeginAnimation(VisualElement target)
    {
        var completion = new TaskCompletionSource();
        if (target is EasingCurveView curve)
        {
            curve.Animate(
                nameof(EasingDemoAnimation),
                v => curve.Progress = v,
                16,
                Duration,
                Easing.Linear,
                (_, _) => completion.TrySetResult());
        }
        else
        {
            completion.TrySetResult();
        }

        return completion.Task;
    }
}
