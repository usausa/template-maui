namespace Template.MobileApp.Behaviors;

public static class GestureOption
{
    //--------------------------------------------------------------------------------
    // MinScale
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty MinScaleProperty = BindableProperty.CreateAttached(
        "MinScale",
        typeof(double),
        typeof(GestureOption),
        1d);

    public static double GetMinScale(BindableObject bindable) => (double)bindable.GetValue(MinScaleProperty);

    public static void SetMinScale(BindableObject bindable, double value) => bindable.SetValue(MinScaleProperty, value);

    //--------------------------------------------------------------------------------
    // MaxScale
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty MaxScaleProperty = BindableProperty.CreateAttached(
        "MaxScale",
        typeof(double),
        typeof(GestureOption),
        4d);

    public static double GetMaxScale(BindableObject bindable) => (double)bindable.GetValue(MaxScaleProperty);

    public static void SetMaxScale(BindableObject bindable, double value) => bindable.SetValue(MaxScaleProperty, value);

    //--------------------------------------------------------------------------------
    // ZoomScale
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ZoomScaleProperty = BindableProperty.CreateAttached(
        "ZoomScale",
        typeof(double),
        typeof(GestureOption),
        1d,
        BindingMode.TwoWay);

    public static double GetZoomScale(BindableObject bindable) => (double)bindable.GetValue(ZoomScaleProperty);

    public static void SetZoomScale(BindableObject bindable, double value) => bindable.SetValue(ZoomScaleProperty, value);

    //--------------------------------------------------------------------------------
    // ZoomOffset
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ZoomOffsetProperty = BindableProperty.CreateAttached(
        "ZoomOffset",
        typeof(Point),
        typeof(GestureOption),
        default(Point),
        BindingMode.TwoWay);

    public static Point GetZoomOffset(BindableObject bindable) => (Point)bindable.GetValue(ZoomOffsetProperty);

    public static void SetZoomOffset(BindableObject bindable, Point value) => bindable.SetValue(ZoomOffsetProperty, value);

    //--------------------------------------------------------------------------------
    // PinchEnabled
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty PinchEnabledProperty = BindableProperty.CreateAttached(
        "PinchEnabled",
        typeof(bool),
        typeof(GestureOption),
        false,
        propertyChanged: OnPinchEnabledChanged);

    public static bool GetPinchEnabled(BindableObject bindable) => (bool)bindable.GetValue(PinchEnabledProperty);

    public static void SetPinchEnabled(BindableObject bindable, bool value) => bindable.SetValue(PinchEnabledProperty, value);

    private static void OnPinchEnabledChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not View view)
        {
            return;
        }

        var recognizer = view.GestureRecognizers.OfType<PinchGestureRecognizer>().FirstOrDefault();
        if ((bool)newValue!)
        {
            if (recognizer is null)
            {
                recognizer = new PinchGestureRecognizer();
                recognizer.PinchUpdated += OnPinchUpdated;
                view.GestureRecognizers.Add(recognizer);
            }
        }
        else if (recognizer is not null)
        {
            recognizer.PinchUpdated -= OnPinchUpdated;
            view.GestureRecognizers.Remove(recognizer);
        }
    }

    private static void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        if ((sender is not View view) || (e.Status != GestureStatus.Running))
        {
            return;
        }

        var current = GetZoomScale(view);
        if ((e.Scale <= 0) || (current <= 0) || (view.Width <= 0) || (view.Height <= 0))
        {
            return;
        }

        var scale = Math.Clamp(current * e.Scale, GetMinScale(view), GetMaxScale(view));

        var ratio = scale / current;
        var offset = GetZoomOffset(view);
        var anchorX = (e.ScaleOrigin.X * view.Width) - (view.Width / 2);
        var anchorY = (e.ScaleOrigin.Y * view.Height) - (view.Height / 2);
        var moved = new Point(
            anchorX - ((anchorX - offset.X) * ratio),
            anchorY - ((anchorY - offset.Y) * ratio));

        SetZoomScale(view, scale);
        SetZoomOffset(view, ClampOffset(view, moved, scale));
    }

    //--------------------------------------------------------------------------------
    // PanEnabled
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty PanEnabledProperty = BindableProperty.CreateAttached(
        "PanEnabled",
        typeof(bool),
        typeof(GestureOption),
        false,
        propertyChanged: OnPanEnabledChanged);

    public static bool GetPanEnabled(BindableObject bindable) => (bool)bindable.GetValue(PanEnabledProperty);

    public static void SetPanEnabled(BindableObject bindable, bool value) => bindable.SetValue(PanEnabledProperty, value);

    private static readonly BindableProperty PanOriginProperty = BindableProperty.CreateAttached(
        "PanOrigin",
        typeof(Point),
        typeof(GestureOption),
        default(Point));

    private static void OnPanEnabledChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not View view)
        {
            return;
        }

        var recognizer = view.GestureRecognizers.OfType<PanGestureRecognizer>().FirstOrDefault();
        if ((bool)newValue!)
        {
            if (recognizer is null)
            {
                recognizer = new PanGestureRecognizer();
                recognizer.PanUpdated += OnPanUpdated;
                view.GestureRecognizers.Add(recognizer);
            }
        }
        else if (recognizer is not null)
        {
            recognizer.PanUpdated -= OnPanUpdated;
            view.GestureRecognizers.Remove(recognizer);
        }
    }

    private static void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (sender is not View view)
        {
            return;
        }

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                view.SetValue(PanOriginProperty, GetZoomOffset(view));
                break;
            case GestureStatus.Running:
                var origin = (Point)view.GetValue(PanOriginProperty);
                var moved = new Point(origin.X + e.TotalX, origin.Y + e.TotalY);
                SetZoomOffset(view, ClampOffset(view, moved, GetZoomScale(view)));
                break;
            case GestureStatus.Canceled:
                SetZoomOffset(view, ClampOffset(view, (Point)view.GetValue(PanOriginProperty), GetZoomScale(view)));
                break;
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static Point ClampOffset(View view, Point offset, double scale)
    {
        var limitX = Math.Max(0, (view.Width * scale) - view.Width) / 2;
        var limitY = Math.Max(0, (view.Height * scale) - view.Height) / 2;
        return new Point(Math.Clamp(offset.X, -limitX, limitX), Math.Clamp(offset.Y, -limitY, limitY));
    }
}
