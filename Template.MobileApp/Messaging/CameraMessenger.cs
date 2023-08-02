namespace Template.MobileApp.Messaging;

//public sealed class CameraCaptureRequest : IEventRequest<CameraCaptureEventArgs>
//{
//    public event EventHandler<CameraCaptureEventArgs>? Requested;

//    public Task<byte[]?> CaptureAsync()
//    {
//        var args = new CameraCaptureEventArgs();
//        Requested?.Invoke(this, args);
//        return args.CompletionSource.Task;
//    }
//}

//public sealed class CameraCaptureEventArgs : EventArgs
//{
//    public TaskCompletionSource<byte[]?> CompletionSource { get; } = new();
//}

public class CameraMessenger
{
    // TODO
}
