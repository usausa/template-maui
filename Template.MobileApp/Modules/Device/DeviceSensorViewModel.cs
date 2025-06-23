namespace Template.MobileApp.Modules.Device;

using System.Numerics;

using Microsoft.Maui.Devices.Sensors;

public sealed partial class DeviceSensorViewModel : AppViewModelBase
{
    private readonly IAccelerometer accelerometer;
    private readonly IBarometer barometer;
    private readonly ICompass compass;
    private readonly IGyroscope gyroscope;
    private readonly IMagnetometer magnetometer;
    private readonly IOrientationSensor orientation;

    [ObservableProperty]
    public partial Vector3 AccelerationValue { get; set; }

    [ObservableProperty]
    public partial double BarometerValue { get; set; }

    [ObservableProperty]
    public partial double MagneticValue { get; set; }

    [ObservableProperty]
    public partial Vector3 GyroscopeValue { get; set; }

    [ObservableProperty]
    public partial Vector3 MagnetometerValue { get; set; }

    [ObservableProperty]
    public partial Quaternion OrientationValue { get; set; }

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

        Disposables.Add(accelerometer.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => AccelerationValue = x.Reading.Acceleration));
        Disposables.Add(barometer.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => BarometerValue = x.Reading.PressureInHectopascals));
        Disposables.Add(compass.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => MagneticValue = x.Reading.HeadingMagneticNorth));
        Disposables.Add(gyroscope.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => GyroscopeValue = x.Reading.AngularVelocity));
        Disposables.Add(magnetometer.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => MagnetometerValue = x.Reading.MagneticField));
        Disposables.Add(orientation.ReadingChangedAsObservable().ObserveOnCurrentContext().Subscribe(x => OrientationValue = x.Reading.Orientation));
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        accelerometer.Start(SensorSpeed.Default);
        barometer.Start(SensorSpeed.Default);
        compass.Start(SensorSpeed.Default);
        gyroscope.Start(SensorSpeed.Default);
        magnetometer.Start(SensorSpeed.Default);
        orientation.Start(SensorSpeed.Default);
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        accelerometer.Stop();
        barometer.Stop();
        compass.Stop();
        gyroscope.Stop();
        magnetometer.Stop();
        orientation.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
