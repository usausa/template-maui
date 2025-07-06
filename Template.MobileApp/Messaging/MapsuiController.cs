namespace Template.MobileApp.Messaging;

public sealed class MoveToEventArgs : EventArgs
{
    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public int? Resolution { get; set; }
}

public sealed class MapsuiController
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<MoveToEventArgs>? MoveToRequest;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<EventArgs>? ZoomInRequest;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<EventArgs>? ZoomOutRequest;

    public double HomeLongitude { get; }

    public double HomeLatitude { get; }

    public int? InitialResolution { get; }

    public MapsuiController(double homeLongitude, double homeLatitude, int? initialResolution = null)
    {
        HomeLongitude = homeLongitude;
        HomeLatitude = homeLatitude;
        InitialResolution = initialResolution;
    }

    public void MoveTo(double longitude, double latitude, int? resolution = null)
    {
        var args = new MoveToEventArgs
        {
            Longitude = longitude,
            Latitude = latitude,
            Resolution = resolution
        };
        MoveToRequest?.Invoke(this, args);
    }

    public void ZoomIn()
    {
        ZoomInRequest?.Invoke(this, EventArgs.Empty);
    }

    public void ZoomOut()
    {
        ZoomOutRequest?.Invoke(this, EventArgs.Empty);
    }
}
