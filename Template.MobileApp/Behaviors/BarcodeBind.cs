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
                    new Binding(nameof(BarcodeController.Enable), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.CameraFacingProperty,
                    new Binding(nameof(BarcodeController.CameraFace), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.CaptureQualityProperty,
                    new Binding(nameof(BarcodeController.CaptureQuality), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.BarcodeSymbologiesProperty,
                    new Binding(nameof(BarcodeController.BarcodeFormat), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.AimModeProperty,
                    new Binding(nameof(BarcodeController.AimMode), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.TorchOnProperty,
                    new Binding(nameof(BarcodeController.TorchOn), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.TapToFocusEnabledProperty,
                    new Binding(nameof(BarcodeController.TapToFocus), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.PauseScanningProperty,
                    new Binding(nameof(BarcodeController.PauseScanning), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.ForceInvertedProperty,
                    new Binding(nameof(BarcodeController.ForceInvert), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.VibrationOnDetectedProperty,
                    new Binding(nameof(BarcodeController.VibrationOnDetect), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.ViewfinderModeProperty,
                    new Binding(nameof(BarcodeController.ViewfinderMode), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.CaptureNextFrameProperty,
                    new Binding(nameof(BarcodeController.CaptureNextFrame), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.ForceFrameCaptureProperty,
                    new Binding(nameof(BarcodeController.ForceFrameCapture), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.PoolingIntervalProperty,
                    new Binding(nameof(BarcodeController.PoolingInterval), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.RequestZoomFactorProperty,
                    new Binding(nameof(BarcodeController.RequestZoomFactor), source: controller));

                AssociatedObject.SetBinding(
                    CameraView.CurrentZoomFactorProperty,
                    new Binding(nameof(BarcodeController.CurrentZoomFactor), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.MinZoomFactorProperty,
                    new Binding(nameof(BarcodeController.MinZoomFactor), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.MaxZoomFactorProperty,
                    new Binding(nameof(BarcodeController.MaxZoomFactor), source: controller));
                AssociatedObject.SetBinding(
                    CameraView.DeviceSwitchZoomFactorProperty,
                    new Binding(nameof(BarcodeController.DeviceSwitchZoomFactor), source: controller));
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
