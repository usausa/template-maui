namespace Template.MobileApp.Modules.Device;

using System.Numerics;

using Microsoft.Maui.Devices.Sensors;

public class DeviceSensorViewModel : AppViewModelBase
{
    private readonly IAccelerometer accelerometer;
    private readonly IBarometer barometer;
    private readonly ICompass compass;
    private readonly IGyroscope gyroscope;
    private readonly IMagnetometer magnetometer;
    private readonly IOrientationSensor orientation;

    public NotificationValue<Vector3> AccelerationValue { get; } = new();

    public NotificationValue<double> BarometerValue { get; } = new();

    public NotificationValue<double> MagneticValue { get; } = new();

    public NotificationValue<Vector3> GyroscopeValue { get; } = new();

    public NotificationValue<Vector3> MagnetometerValue { get; } = new();

    public NotificationValue<Quaternion> OrientationValue { get; } = new();

    public DeviceSensorViewModel(
        ApplicationState applicationState,
        IAccelerometer accelerometer,
        IBarometer barometer,
        ICompass compass,
        IGyroscope gyroscope,
        IMagnetometer magnetometer,
        IOrientationSensor orientation)
        : base(applicationState)
    {
        this.accelerometer = accelerometer;
        this.barometer = barometer;
        this.compass = compass;
        this.gyroscope = gyroscope;
        this.magnetometer = magnetometer;
        this.orientation = orientation;

        Disposables.Add(accelerometer.ObserveReadingChangedOnCurrentContext().Subscribe(
            x => AccelerationValue.Value = x.Reading.Acceleration));
        Disposables.Add(barometer.ObserveReadingChangedOnCurrentContext().Subscribe(
            x => BarometerValue.Value = x.Reading.PressureInHectopascals));
        Disposables.Add(compass.ObserveReadingChangedOnCurrentContext().Subscribe(
            x => MagneticValue.Value = x.Reading.HeadingMagneticNorth));
        Disposables.Add(gyroscope.ObserveReadingChangedOnCurrentContext().Subscribe(
            x => GyroscopeValue.Value = x.Reading.AngularVelocity));
        Disposables.Add(magnetometer.ObserveReadingChangedOnCurrentContext().Subscribe(
            x => MagnetometerValue.Value = x.Reading.MagneticField));
        Disposables.Add(orientation.ObserveReadingChangedOnCurrentContext().Subscribe(
            x => OrientationValue.Value = x.Reading.Orientation));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    public override void OnNavigatedTo(INavigationContext context)
    {
        accelerometer.Start(SensorSpeed.Default);
        barometer.Start(SensorSpeed.Default);
        compass.Start(SensorSpeed.Default);
        gyroscope.Start(SensorSpeed.Default);
        magnetometer.Start(SensorSpeed.Default);
        orientation.Start(SensorSpeed.Default);
    }

    public override void OnNavigatingFrom(INavigationContext context)
    {
        accelerometer.Stop();
        barometer.Stop();
        compass.Stop();
        gyroscope.Stop();
        magnetometer.Stop();
        orientation.Stop();
    }
}
