namespace Template.MobileApp.Messaging;

using Camera.MAUI;

using Template.MobileApp.Helpers;

public sealed class CameraPreviewEventArgs : TaskEventArgs<bool>
{
    public bool Enable { get; set; }
}

public sealed class CameraPositionEventArgs : TaskEventArgs
{
    public CameraPosition? Position { get; set; }
}

public sealed class CameraTakePhotoEventArgs : TaskEventArgs<Stream?>
{
    public ImageFormat Format { get; set; } = ImageFormat.JPEG;
}

public sealed class CameraSaveSnapshotEventArgs : TaskEventArgs<bool>
{
    public string Path { get; set; } = default!;

    public ImageFormat Format { get; set; } = ImageFormat.JPEG;
}

public interface ICameraController
{
    event EventHandler<CameraPreviewEventArgs> PreviewRequest;

    event EventHandler<CameraPositionEventArgs> PositionRequest;

    event EventHandler<CameraTakePhotoEventArgs> TakePhotoRequest;

    event EventHandler<CameraSaveSnapshotEventArgs> SaveSnapshotRequest;

    event EventHandler<EventArgs> FocusRequest;

    // Property

    CameraPosition? DefaultPosition { get; }

    CameraInfo? Camera { get; }

    bool Torch { get; set; }

    bool Mirror { get; set; }

    FlashMode FlashMode { get; set; }

    float Zoom { get; set; }

    bool BarcodeDetection { get; set; }

    // Event

    void UpdateCamera(CameraInfo? value);

    void HandleBarcodeDetected(BarcodeResult result);
}

public sealed class CameraController : NotificationObject, ICameraController
{
    private event EventHandler<CameraPreviewEventArgs>? PreviewRequestHandler;

    private event EventHandler<CameraPositionEventArgs>? PositionRequestHandler;

    private event EventHandler<CameraTakePhotoEventArgs>? TakePhotoRequestHandler;

    private event EventHandler<CameraSaveSnapshotEventArgs>? SaveSnapshotRequestHandler;

    private event EventHandler<EventArgs>? FocusRequestHandler;

    event EventHandler<CameraPreviewEventArgs> ICameraController.PreviewRequest
    {
        add => PreviewRequestHandler += value;
        remove => PreviewRequestHandler -= value;
    }

    event EventHandler<CameraPositionEventArgs> ICameraController.PositionRequest
    {
        add => PositionRequestHandler += value;
        remove => PositionRequestHandler -= value;
    }

    event EventHandler<CameraTakePhotoEventArgs> ICameraController.TakePhotoRequest
    {
        add => TakePhotoRequestHandler += value;
        remove => TakePhotoRequestHandler -= value;
    }

    event EventHandler<CameraSaveSnapshotEventArgs> ICameraController.SaveSnapshotRequest
    {
        add => SaveSnapshotRequestHandler += value;
        remove => SaveSnapshotRequestHandler -= value;
    }

    event EventHandler<EventArgs> ICameraController.FocusRequest
    {
        add => FocusRequestHandler += value;
        remove => FocusRequestHandler -= value;
    }

    // Field

    private readonly ICommand? command;

    private readonly CameraPosition? defaultPosition;

    // Property

    CameraPosition? ICameraController.DefaultPosition => defaultPosition;

    public CameraInfo? Camera { get; private set; }

    public bool Torch
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool Mirror
    {
        get;
        set => SetProperty(ref field, value);
    }

    public FlashMode FlashMode
    {
        get;
        set => SetProperty(ref field, value);
    }

    public float Zoom
    {
        get;
        set
        {
            if (Camera is null)
            {
                value = 1f;
            }
            else
            {
                if (value < Camera.MinZoomFactor)
                {
                    value = Camera.MinZoomFactor;
                }
                else if (value > Camera.MaxZoomFactor)
                {
                    value = Camera.MaxZoomFactor;
                }
            }

            SetProperty(ref field, value);
        }
    }

    public bool BarcodeDetection
    {
        get;
        set => SetProperty(ref field, value);
    }

    // Constructor

    public CameraController()
    {
        Zoom = 1f;
    }

    public CameraController(CameraPosition position)
        : this()
    {
        defaultPosition = position;
    }

    public CameraController(ICommand command)
        : this()
    {
        this.command = command;
    }

    public CameraController(CameraPosition position, ICommand command)
        : this()
    {
        defaultPosition = position;
        this.command = command;
    }

    // Message

    public Task<bool> StartPreviewAsync()
    {
        var args = new CameraPreviewEventArgs { Enable = true };
        PreviewRequestHandler?.Invoke(this, args);
        return args.Task;
    }

    public Task<bool> StopPreviewAsync()
    {
        var args = new CameraPreviewEventArgs();
        PreviewRequestHandler?.Invoke(this, args);
        return args.Task;
    }

    public Task ResetPositionAsync()
    {
        var args = new CameraPositionEventArgs { Position = defaultPosition };
        PositionRequestHandler?.Invoke(this, args);
        return args.Task;
    }

    public Task SwitchPositionAsync(CameraPosition? position = null)
    {
        var args = new CameraPositionEventArgs { Position = position };
        PositionRequestHandler?.Invoke(this, args);
        return args.Task;
    }

    public Task<Stream?> TakePhotoAsync(ImageFormat imageFormat = ImageFormat.JPEG)
    {
        var args = new CameraTakePhotoEventArgs { Format = imageFormat };
        TakePhotoRequestHandler?.Invoke(this, args);
        return args.Task;
    }

    public Task<bool> SaveSnapshotAsync(string path, ImageFormat imageFormat = ImageFormat.JPEG)
    {
        var args = new CameraSaveSnapshotEventArgs { Path = path, Format = imageFormat };
        SaveSnapshotRequestHandler?.Invoke(this, args);
        return args.Task;
    }

    public void FocusRequest()
    {
        FocusRequestHandler?.Invoke(this, EventArgs.Empty);
    }

    // Event

    void ICameraController.UpdateCamera(CameraInfo? value)
    {
        Camera = value;
        RaisePropertyChanged(nameof(Camera));
    }

    void ICameraController.HandleBarcodeDetected(BarcodeResult result)
    {
        if ((command is not null) && command.CanExecute(result))
        {
            command.Execute(result);
        }
    }
}
