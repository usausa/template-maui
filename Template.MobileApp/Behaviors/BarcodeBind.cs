namespace Template.MobileApp.Behaviors;

using BarcodeScanning;

using Smart.Maui.Interactivity;

public static class BarcodeBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(BarcodeController),
        typeof(BarcodeBind),
        null,
        propertyChanged: BindChanged);

    public static BarcodeController? GetController(BindableObject bindable) =>
        (BarcodeController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, BarcodeController? value) =>
        bindable.SetValue(ControllerProperty, value);

    private static void BindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not CameraView view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is BarcodeBindBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new BarcodeBindBehavior());
        }
    }

    private sealed class BarcodeBindBehavior : BehaviorBase<CameraView>
    {
        protected override void OnAttachedTo(CameraView bindable)
        {
            base.OnAttachedTo(bindable);

            var controller = GetController(bindable);
            if ((controller is not null) && (AssociatedObject is not null))
            {
                AssociatedObject.SetBinding(
                    CameraView.CameraEnabledProperty,
                    static (BarcodeController controller) => controller.Enable,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.CameraFacingProperty,
                    static (BarcodeController controller) => controller.CameraFace,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.CaptureQualityProperty,
                    static (BarcodeController controller) => controller.CaptureQuality,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.BarcodeSymbologiesProperty,
                    static (BarcodeController controller) => controller.BarcodeFormat,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.AimModeProperty,
                    static (BarcodeController controller) => controller.AimMode,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.TorchOnProperty,
                    static (BarcodeController controller) => controller.TorchOn,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.TapToFocusEnabledProperty,
                    static (BarcodeController controller) => controller.TapToFocus,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.PauseScanningProperty,
                    static (BarcodeController controller) => controller.PauseScanning,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.ForceInvertedProperty,
                    static (BarcodeController controller) => controller.ForceInvert,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.VibrationOnDetectedProperty,
                    static (BarcodeController controller) => controller.VibrationOnDetect,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.ViewfinderModeProperty,
                    static (BarcodeController controller) => controller.ViewfinderMode,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.CaptureNextFrameProperty,
                    static (BarcodeController controller) => controller.CaptureNextFrame,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.ForceFrameCaptureProperty,
                    static (BarcodeController controller) => controller.ForceFrameCapture,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.PoolingIntervalProperty,
                    static (BarcodeController controller) => controller.PoolingInterval,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.RequestZoomFactorProperty,
                    static (BarcodeController controller) => controller.RequestZoomFactor,
                    source: controller);

                AssociatedObject.SetBinding(
                    CameraView.CurrentZoomFactorProperty,
                    static (BarcodeController controller) => controller.CurrentZoomFactor,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.MinZoomFactorProperty,
                    static (BarcodeController controller) => controller.MinZoomFactor,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.MaxZoomFactorProperty,
                    static (BarcodeController controller) => controller.MaxZoomFactor,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.DeviceSwitchZoomFactorProperty,
                    static (BarcodeController controller) => controller.DeviceSwitchZoomFactor,
                    source: controller);
            }
        }

        protected override void OnDetachingFrom(CameraView bindable)
        {
            bindable.RemoveBinding(CameraView.CameraEnabledProperty);
            bindable.RemoveBinding(CameraView.CameraFacingProperty);
            bindable.RemoveBinding(CameraView.CaptureQualityProperty);
            bindable.RemoveBinding(CameraView.BarcodeSymbologiesProperty);
            bindable.RemoveBinding(CameraView.AimModeProperty);
            bindable.RemoveBinding(CameraView.TorchOnProperty);
            bindable.RemoveBinding(CameraView.TapToFocusEnabledProperty);
            bindable.RemoveBinding(CameraView.PauseScanningProperty);
            bindable.RemoveBinding(CameraView.ForceInvertedProperty);
            bindable.RemoveBinding(CameraView.VibrationOnDetectedProperty);
            bindable.RemoveBinding(CameraView.ViewfinderModeProperty);
            bindable.RemoveBinding(CameraView.CaptureNextFrameProperty);
            bindable.RemoveBinding(CameraView.ForceFrameCaptureProperty);
            bindable.RemoveBinding(CameraView.PoolingIntervalProperty);
            bindable.RemoveBinding(CameraView.RequestZoomFactorProperty);

            bindable.RemoveBinding(CameraView.CurrentZoomFactorProperty);
            bindable.RemoveBinding(CameraView.MinZoomFactorProperty);
            bindable.RemoveBinding(CameraView.MaxZoomFactorProperty);
            bindable.RemoveBinding(CameraView.DeviceSwitchZoomFactorProperty);

            base.OnDetachingFrom(bindable);
        }
    }
}
