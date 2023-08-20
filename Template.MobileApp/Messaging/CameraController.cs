namespace Template.MobileApp.Messaging;

using Camera.MAUI;

using Template.MobileApp.Helpers;

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
    event EventHandler<CameraPositionEventArgs> PositionRequest;

    event EventHandler<CameraTakePhotoEventArgs> TakePhotoRequest;

    event EventHandler<CameraSaveSnapshotEventArgs> SaveSnapshotRequest;

    event EventHandler<EventArgs> FocusRequest;

    CameraPosition? DefaultPosition { get; }

    CameraInfo? Camera { get; }

    void UpdateCamera(CameraInfo? value);

    bool Preview { get; set; }

    bool Torch { get; set; }

    bool Mirror { get; set; }

    FlashMode FlashMode { get; set; }

    float Zoom { get; set; }

    bool BarcodeDetection { get; set; }

    Task<Stream?> TakePhotoAsync(ImageFormat imageFormat = ImageFormat.JPEG);

    Task<bool> SaveSnapshotAsync(string path, ImageFormat imageFormat = ImageFormat.JPEG);
}

public sealed class CameraController : NotificationObject, ICameraController
{
    private event EventHandler<CameraPositionEventArgs>? PositionRequestHandler;

    private event EventHandler<CameraTakePhotoEventArgs>? TakePhotoRequestHandler;

    private event EventHandler<CameraSaveSnapshotEventArgs>? SaveSnapshotRequestHandler;

    private event EventHandler<EventArgs>? FocusRequestHandler;

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

    private readonly CameraPosition? defaultPosition;

    CameraPosition? ICameraController.DefaultPosition => defaultPosition;

    public CameraInfo? Camera { get; private set; }

    private bool preview;

    public bool Preview
    {
        get => preview;
        set => SetProperty(ref preview, value);
    }

    private bool torch;

    public bool Torch
    {
        get => torch;
        set => SetProperty(ref torch, value);
    }

    private bool mirror;

    public bool Mirror
    {
        get => mirror;
        set => SetProperty(ref mirror, value);
    }

    private FlashMode flashMode;

    public FlashMode FlashMode
    {
        get => flashMode;
        set => SetProperty(ref flashMode, value);
    }

    private float zoom;

    public float Zoom
    {
        get => zoom;
        set => SetProperty(ref zoom, value);
    }

    private bool barcodeDetection;

    public bool BarcodeDetection
    {
        get => barcodeDetection;
        set => SetProperty(ref barcodeDetection, value);
    }

    public CameraController(CameraPosition? position = null)
    {
        defaultPosition = position;
    }

    public async Task ResetPositionAsync()
    {
        var args = new CameraPositionEventArgs { Position = defaultPosition };
        PositionRequestHandler?.Invoke(this, args);
        await args.Task;
    }

    public Task SwitchPositionAsync(CameraPosition? position = null)
    {
        var args = new CameraPositionEventArgs { Position = position };
        PositionRequestHandler?.Invoke(this, args);
        return args.Task;
    }

    void ICameraController.UpdateCamera(CameraInfo? value)
    {
        Camera = value;
        RaisePropertyChanged(nameof(Camera));
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
}
