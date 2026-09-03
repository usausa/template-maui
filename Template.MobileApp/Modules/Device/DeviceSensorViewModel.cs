namespace Template.MobileApp.Modules.Device;

using System.Numerics;

using Microsoft.Maui.Devices.Sensors;

using Template.MobileApp.Graphics.Drawing;

public sealed partial class DeviceSensorViewModel : AppViewModelBase
{
    private readonly IAccelerometer accelerometer;
    private readonly IBarometer barometer;
    private readonly ICompass compass;
    private readonly IGyroscope gyroscope;
    private readonly IMagnetometer magnetometer;
    private readonly IOrientationSensor orientation;

    public CompassDrawing CompassDial { get; } = new();

    public LevelDrawing Level { get; } = new();

    [ObservableProperty(NotifyAlso = [nameof(AccelerationX), nameof(AccelerationY), nameof(AccelerationZ)])]
    public partial Vector3 AccelerationValue { get; set; }

    public double AccelerationX => AccelerationValue.X;
    public double AccelerationY => AccelerationValue.Y;
    public double AccelerationZ => AccelerationValue.Z;

    [ObservableProperty]
    public partial double BarometerValue { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(CompassDirection)])]
    public partial double MagneticValue { get; set; }

    public string CompassDirection => ((int)Math.Round(MagneticValue / 45d) % 8) switch
    {
        1 => "NE",
        2 => "E",
        3 => "SE",
        4 => "S",
        5 => "SW",
        6 => "W",
        7 => "NW",
        _ => "N"
    };

    [ObservableProperty(NotifyAlso = [nameof(GyroscopeX), nameof(GyroscopeY), nameof(GyroscopeZ)])]
    public partial Vector3 GyroscopeValue { get; set; }

    public double GyroscopeX => GyroscopeValue.X;
    public double GyroscopeY => GyroscopeValue.Y;
    public double GyroscopeZ => GyroscopeValue.Z;

    [ObservableProperty(NotifyAlso = [nameof(MagnetometerX), nameof(MagnetometerY), nameof(MagnetometerZ)])]
    public partial Vector3 MagnetometerValue { get; set; }

    public double MagnetometerX => MagnetometerValue.X;
    public double MagnetometerY => MagnetometerValue.Y;
    public double MagnetometerZ => MagnetometerValue.Z;

    [ObservableProperty(NotifyAlso = [nameof(OrientationX), nameof(OrientationY), nameof(OrientationZ), nameof(OrientationW)])]
    public partial Quaternion OrientationValue { get; set; }

    public double OrientationX => OrientationValue.X;
    public double OrientationY => OrientationValue.Y;
    public double OrientationZ => OrientationValue.Z;
    public double OrientationW => OrientationValue.W;

    public DeviceSensorViewModel(
        IAccelerometer accelerometer,
        IBarometer barometer,
        ICompass compass,
        IGyroscope gyroscope,
        IMagnetometer magnetometer,
        IOrientationSensor orientation)
    {
        this.accelerometer = accelerometer;
        this.barometer = barometer;
        this.compass = compass;
        this.gyroscope = gyroscope;
        this.magnetometer = magnetometer;
        this.orientation = orientation;

        Disposables.Add(accelerometer.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x =>
        {
            AccelerationValue = x.Reading.Acceleration;
            Level.GravityX = x.Reading.Acceleration.X;
            Level.GravityY = x.Reading.Acceleration.Y;
            Level.Invalidate();
        }));
        Disposables.Add(barometer.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => BarometerValue = x.Reading.PressureInHectopascals));
        Disposables.Add(compass.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x =>
        {
            MagneticValue = x.Reading.HeadingMagneticNorth;
            CompassDial.Heading = (float)x.Reading.HeadingMagneticNorth;
            CompassDial.Invalidate();
        }));
        Disposables.Add(gyroscope.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => GyroscopeValue = x.Reading.AngularVelocity));
        Disposables.Add(magnetometer.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => MagnetometerValue = x.Reading.MagneticField));
        Disposables.Add(orientation.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => OrientationValue = x.Reading.Orientation));
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        // 非搭載センサーはFeatureNotSupportedExceptionになるため開始しない (表示は初期値のまま)
        if (accelerometer.IsSupported)
        {
            accelerometer.Start(SensorSpeed.Default);
        }
        if (barometer.IsSupported)
        {
            barometer.Start(SensorSpeed.Default);
        }
        if (compass.IsSupported)
        {
            compass.Start(SensorSpeed.Default);
        }
        if (gyroscope.IsSupported)
        {
            gyroscope.Start(SensorSpeed.Default);
        }
        if (magnetometer.IsSupported)
        {
            magnetometer.Start(SensorSpeed.Default);
        }
        if (orientation.IsSupported)
        {
            orientation.Start(SensorSpeed.Default);
        }
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        if (accelerometer.IsMonitoring)
        {
            accelerometer.Stop();
        }
        if (barometer.IsMonitoring)
        {
            barometer.Stop();
        }
        if (compass.IsMonitoring)
        {
            compass.Stop();
        }
        if (gyroscope.IsMonitoring)
        {
            gyroscope.Stop();
        }
        if (magnetometer.IsMonitoring)
        {
            magnetometer.Stop();
        }
        if (orientation.IsMonitoring)
        {
            orientation.Stop();
        }
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
