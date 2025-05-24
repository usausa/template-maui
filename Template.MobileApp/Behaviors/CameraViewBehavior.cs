namespace Template.MobileApp.Behaviors;

using Camera.MAUI;

using Smart.Maui.Interactivity;

public sealed class CameraViewBehavior : BehaviorBase<CameraView>
{
    protected override void OnAttachedTo(CameraView bindable)
    {
        base.OnAttachedTo(bindable);

        bindable.CamerasLoaded += BindableOnCamerasLoaded;
        // TODO
    }

    protected override void OnDetachingFrom(CameraView bindable)
    {
        bindable.CamerasLoaded -= BindableOnCamerasLoaded;

        base.OnDetachingFrom(bindable);
    }

    private void BindableOnCamerasLoaded(object? sender, EventArgs e)
    {
        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.Camera = AssociatedObject.Cameras.FirstOrDefault();
        AssociatedObject.AutoStartPreview = true;
    }
}
