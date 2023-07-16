namespace Template.MobileApp.Modules.Device;

using System.Numerics;

using Microsoft.Maui.Devices.Sensors;

using Template.MobileApp;

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

        Disposables.Add(Observable
            .FromEvent<EventHandler<AccelerometerChangedEventArgs>, AccelerometerChangedEventArgs>(h => (_, e) => h(e), h => accelerometer.ReadingChanged += h, h => accelerometer.ReadingChanged -= h)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(x => AccelerationValue.Value = x.Reading.Acceleration));
        Disposables.Add(Observable
            .FromEvent<EventHandler<BarometerChangedEventArgs>, BarometerChangedEventArgs>(h => (_, e) => h(e), h => barometer.ReadingChanged += h, h => barometer.ReadingChanged -= h)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(x => BarometerValue.Value = x.Reading.PressureInHectopascals));
        Disposables.Add(Observable
            .FromEvent<EventHandler<CompassChangedEventArgs>, CompassChangedEventArgs>(h => (_, e) => h(e), h => compass.ReadingChanged += h, h => compass.ReadingChanged -= h)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(x => MagneticValue.Value = x.Reading.HeadingMagneticNorth));
        Disposables.Add(Observable
            .FromEvent<EventHandler<GyroscopeChangedEventArgs>, GyroscopeChangedEventArgs>(h => (_, e) => h(e), h => gyroscope.ReadingChanged += h, h => gyroscope.ReadingChanged -= h)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(x => GyroscopeValue.Value = x.Reading.AngularVelocity));
        Disposables.Add(Observable
            .FromEvent<EventHandler<MagnetometerChangedEventArgs>, MagnetometerChangedEventArgs>(h => (_, e) => h(e), h => magnetometer.ReadingChanged += h, h => magnetometer.ReadingChanged -= h)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(x => MagnetometerValue.Value = x.Reading.MagneticField));
        Disposables.Add(Observable
            .FromEvent<EventHandler<OrientationSensorChangedEventArgs>, OrientationSensorChangedEventArgs>(h => (_, e) => h(e), h => orientation.ReadingChanged += h, h => orientation.ReadingChanged -= h)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(x => OrientationValue.Value = x.Reading.Orientation));
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
