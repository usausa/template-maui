namespace Template.MobileApp.Behaviors;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

using Smart.Maui.Interactivity;

public static class CameraBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(CameraController),
        typeof(CameraBind),
        null,
        propertyChanged: BindChanged);

    public static CameraController? GetController(BindableObject bindable) =>
        (CameraController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, CameraController? value) =>
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
        private CameraController? controller;

        protected override void OnAttachedTo(CameraView bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            if ((controller is not null) && (AssociatedObject is not null))
            {
                controller.GetAvailableListRequest += OnGetAvailableListRequest;
                controller.PreviewRequest += OnPreviewRequest;
                controller.CaptureRequest += OnCaptureRequest;

                AssociatedObject.SetBinding(
                    CameraView.IsAvailableProperty,
                    static (CameraController controller) => controller.IsAvailable,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.IsBusyProperty,
                    static (CameraController controller) => controller.IsBusy,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.SelectedCameraProperty,
                    static (CameraController controller) => controller.Selected,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.CameraFlashModeProperty,
                    static (CameraController controller) => controller.CameraFlashMode,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.ImageCaptureResolutionProperty,
                    static (CameraController controller) => controller.CaptureResolution,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.ZoomFactorProperty,
                    static (CameraController controller) => controller.ZoomFactor,
                    source: controller);
                AssociatedObject.SetBinding(
                    CameraView.IsTorchOnProperty,
                    static (CameraController controller) => controller.IsTorchOn,
                    source: controller);
            }
        }

        protected override void OnDetachingFrom(CameraView bindable)
        {
            bindable.RemoveBinding(CameraView.IsAvailableProperty);
            bindable.RemoveBinding(CameraView.IsBusyProperty);
            bindable.RemoveBinding(CameraView.SelectedCameraProperty);
            bindable.RemoveBinding(CameraView.CameraFlashModeProperty);
            bindable.RemoveBinding(CameraView.ImageCaptureResolutionProperty);
            bindable.RemoveBinding(CameraView.ZoomFactorProperty);
            bindable.RemoveBinding(CameraView.IsTorchOnProperty);

            if (controller is not null)
            {
                controller.GetAvailableListRequest -= OnGetAvailableListRequest;
                controller.PreviewRequest -= OnPreviewRequest;
                controller.CaptureRequest -= OnCaptureRequest;
            }

            controller = null;

            base.OnDetachingFrom(bindable);
        }

        private void OnGetAvailableListRequest(object? sender, CameraGetAvailableListEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }

#pragma warning disable CA2012
            e.Task = GetAvailableCameras(AssociatedObject);
#pragma warning restore CA2012
        }

        private void OnPreviewRequest(object? sender, CameraPreviewEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }

#pragma warning disable CA2012
            e.Task = e.Enable ? StartCameraPreview(AssociatedObject) : StopCameraPreview(AssociatedObject);
#pragma warning restore CA2012
        }

        private void OnCaptureRequest(object? sender, CameraCaptureEventArgs e)
        {
            var cameraView = AssociatedObject;
            if (cameraView is null)
            {
                return;
            }

            var capture = new CaptureObject(cameraView);
#pragma warning disable CA2012
            e.Task = capture.CaptureAsync(e.Token);
#pragma warning restore CA2012
        }

        private static ValueTask<IReadOnlyList<CameraInfo>> GetAvailableCameras(CameraView cameraView)
        {
            return cameraView.GetAvailableCameras(CancellationToken.None);
        }

        private static async ValueTask StartCameraPreview(CameraView cameraView)
        {
            await cameraView.StartCameraPreview(CancellationToken.None);
        }

        private static ValueTask StopCameraPreview(CameraView cameraView)
        {
            cameraView.StopCameraPreview();
            return ValueTask.CompletedTask;
        }

        private sealed class CaptureObject
        {
            private readonly TaskCompletionSource<Stream?> result = new();

            private readonly CameraView view;

            public CaptureObject(CameraView view)
            {
                this.view = view;
            }

            public async ValueTask<Stream?> CaptureAsync(CancellationToken token)
            {
                view.MediaCaptured += OnMediaCaptured;
                view.MediaCaptureFailed += OnMediaCaptureFailed;
                await view.CaptureImage(token);
                return await result.Task;
            }

            private void OnMediaCaptured(object? sender, MediaCapturedEventArgs e) => OnMediaCaptured(e.Media);

            private void OnMediaCaptureFailed(object? sender, MediaCaptureFailedEventArgs e) => OnMediaCaptured(null);

            private void OnMediaCaptured(Stream? stream)
            {
                view.MediaCaptured -= OnMediaCaptured;
                view.MediaCaptureFailed -= OnMediaCaptureFailed;
                result.TrySetResult(stream);
            }
        }
    }
}
