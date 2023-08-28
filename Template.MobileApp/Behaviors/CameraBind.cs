namespace Template.MobileApp.Behaviors;

using Camera.MAUI;
using Camera.MAUI.ZXingHelper;

using Smart.Maui.Interactivity;

public static class CameraBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(ICameraController),
        typeof(CameraBind),
        null,
        propertyChanged: BindChanged);

    public static ICameraController GetController(BindableObject bindable) =>
        (ICameraController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, ICameraController value) =>
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
            if (controller is not null)
            {
                controller.PreviewRequest += ControllerOnPreviewRequest;
                controller.PositionRequest += ControllerOnPositionRequest;
                controller.TakePhotoRequest += ControllerOnTakePhotoRequest;
                controller.SaveSnapshotRequest += ControllerOnSaveSnapshotRequest;
                controller.FocusRequest += ControllerOnFocusRequest;
            }

            bindable.CamerasLoaded += BindableOnCamerasLoaded;
            bindable.BarcodeDetected += BindableOnBarcodeDetected;
        }

        protected override void OnDetachingFrom(CameraView bindable)
        {
            if (controller is not null)
            {
                controller.PreviewRequest -= ControllerOnPreviewRequest;
                controller.PositionRequest -= ControllerOnPositionRequest;
                controller.TakePhotoRequest -= ControllerOnTakePhotoRequest;
                controller.SaveSnapshotRequest -= ControllerOnSaveSnapshotRequest;
                controller.FocusRequest -= ControllerOnFocusRequest;
            }

            bindable.CamerasLoaded -= BindableOnCamerasLoaded;
            bindable.BarcodeDetected -= BindableOnBarcodeDetected;

            bindable.RemoveBinding(CameraView.CameraProperty);
            bindable.RemoveBinding(CameraView.TorchEnabledProperty);
            bindable.RemoveBinding(CameraView.MirroredImageProperty);
            bindable.RemoveBinding(CameraView.FlashModeProperty);
            bindable.RemoveBinding(CameraView.ZoomFactorProperty);
            bindable.RemoveBinding(CameraView.BarCodeDetectionEnabledProperty);

            controller = null;

            base.OnDetachingFrom(bindable);
        }

        private void BindableOnCamerasLoaded(object? sender, EventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (controller is null)
            {
                return;
            }

            var camera = controller.DefaultPosition is not null
                ? AssociatedObject.Cameras.FirstOrDefault(x => x.Position == controller.DefaultPosition)
                : AssociatedObject.Cameras.FirstOrDefault();
            AssociatedObject.Camera = camera;

            AssociatedObject.SetBinding(
                CameraView.TorchEnabledProperty,
                new Binding(nameof(ICameraController.Torch), source: controller));
            AssociatedObject.SetBinding(
                CameraView.MirroredImageProperty,
                new Binding(nameof(ICameraController.Mirror), source: controller));
            AssociatedObject.SetBinding(
                CameraView.FlashModeProperty,
                new Binding(nameof(ICameraController.FlashMode), source: controller));
            AssociatedObject.SetBinding(
                CameraView.ZoomFactorProperty,
                new Binding(nameof(ICameraController.Zoom), source: controller));
            AssociatedObject.SetBinding(
                CameraView.BarCodeDetectionEnabledProperty,
                new Binding(nameof(ICameraController.BarcodeDetection), source: controller));

            controller.UpdateCamera(camera);
        }

        private void BindableOnBarcodeDetected(object sender, BarcodeEventArgs args)
        {
            if ((controller is null) || (args.Result.Length == 0))
            {
                return;
            }

            controller.HandleBarcodeDetected(args.Result[0]);
        }

        private void ControllerOnPreviewRequest(object? sender, CameraPreviewEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            e.Task = e.Enable ? StartCameraPreview(AssociatedObject) : StopCameraPreview(AssociatedObject);
        }

        private static async Task<bool> StartCameraPreview(CameraView cameraView)
        {
            var result = await cameraView.StartCameraAsync();
            return result == CameraResult.Success;
        }

        private static async Task<bool> StopCameraPreview(CameraView cameraView)
        {
            var result = await cameraView.StopCameraAsync();
            return result == CameraResult.Success;
        }

        private void ControllerOnPositionRequest(object? sender, CameraPositionEventArgs e)
        {
            if ((AssociatedObject is null) || (controller is null))
            {
                return;
            }

            e.Task = PositionRequest(AssociatedObject, controller, e.Position);
        }

        private static Task PositionRequest(CameraView cameraView, ICameraController controller, CameraPosition? position)
        {
            CameraInfo? newCamera;
            if (position is not null)
            {
                newCamera = cameraView.Cameras.FirstOrDefault(x => x.Position == position);
            }
            else
            {
                var current = cameraView.Cameras.IndexOf(cameraView.Camera);
                newCamera = (current >= 0) && (current + 1 < cameraView.Cameras.Count)
                    ? cameraView.Cameras[current + 1]
                    : cameraView.Cameras.FirstOrDefault();
            }

            if (cameraView.Camera != newCamera)
            {
                cameraView.Camera = newCamera;

                controller.UpdateCamera(newCamera);
            }

            return Task.CompletedTask;
        }

        private void ControllerOnTakePhotoRequest(object? sender, CameraTakePhotoEventArgs e)
        {
            var cameraView = AssociatedObject;
            if (cameraView is null)
            {
                return;
            }

            e.Task = cameraView.TakePhotoAsync(e.Format);
        }

        private void ControllerOnSaveSnapshotRequest(object? sender, CameraSaveSnapshotEventArgs e)
        {
            var cameraView = AssociatedObject;
            if (cameraView is null)
            {
                return;
            }

            e.Task = cameraView.SaveSnapShot(e.Format, e.Path);
        }

        private void ControllerOnFocusRequest(object? sender, EventArgs e)
        {
            AssociatedObject?.ForceAutoFocus();
        }
    }
}
