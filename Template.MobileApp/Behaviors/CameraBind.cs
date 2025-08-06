namespace Template.MobileApp.Behaviors;

using CommunityToolkit.Maui.Views;

using Smart.Maui.Interactivity;

public static class CameraBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(ICameraController),
        typeof(CameraBind),
        null,
        propertyChanged: BindChanged);

    public static ICameraController? GetController(BindableObject bindable) =>
        (ICameraController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, ICameraController? value) =>
        bindable.SetValue(ControllerProperty, value);

    private static void BindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not CameraView view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is CameraBindBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new CameraBindBehavior());
        }
    }

    private sealed class CameraBindBehavior : BehaviorBase<CameraView>
    {
        private ICameraController? controller;

        protected override void OnAttachedTo(CameraView bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            if ((controller is not null) && (AssociatedObject is not null))
            {
                controller.Attach(bindable);

                AssociatedObject.SetBinding(
                    CameraView.IsAvailableProperty,
                    static (ICameraController controller) => controller.IsAvailable,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.IsCameraBusyProperty,
                    static (ICameraController controller) => controller.IsCameraBusy,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.SelectedCameraProperty!,
                    static (ICameraController controller) => controller.Selected,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.CameraFlashModeProperty,
                    static (ICameraController controller) => controller.CameraFlashMode,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.ImageCaptureResolutionProperty,
                    static (ICameraController controller) => controller.CaptureResolution,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.ZoomFactorProperty,
                    static (ICameraController controller) => controller.ZoomFactor,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.IsTorchOnProperty,
                    static (ICameraController controller) => controller.IsTorchOn,
                    source: controller);
            }
        }

        protected override void OnDetachingFrom(CameraView bindable)
        {
            controller?.Detach();
            controller = null;

            bindable.RemoveBinding(CameraView.IsAvailableProperty);
            bindable.RemoveBinding(CameraView.IsCameraBusyProperty);
            bindable.RemoveBinding(CameraView.SelectedCameraProperty);
            bindable.RemoveBinding(CameraView.CameraFlashModeProperty);
            bindable.RemoveBinding(CameraView.ImageCaptureResolutionProperty);
            bindable.RemoveBinding(CameraView.ZoomFactorProperty);
            bindable.RemoveBinding(CameraView.IsTorchOnProperty);

            controller = null;

            base.OnDetachingFrom(bindable);
        }
    }
}
