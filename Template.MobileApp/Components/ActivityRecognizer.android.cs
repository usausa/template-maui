namespace Template.MobileApp.Components;

using Android.App;
using Android.Content;
using Android.Hardware;

public sealed partial class ActivityRecognizer : Java.Lang.Object, ISensorEventListener
{
    private SensorManager? sensorManager;

    private Sensor? stepCounter;

    private bool started;

    private int baseCount;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Enabled)
            {
                Enabled = false;
            }

            sensorManager?.Dispose();
            sensorManager = null;
        }
        base.Dispose(disposing);
    }

    // 権限のCheck→Requestは呼び出し側で実施する (ActivityRecognitionPermission)
    private partial void Start()
    {
        sensorManager ??= (SensorManager)Application.Context.GetSystemService(Context.SensorService)!;
        stepCounter ??= sensorManager.GetDefaultSensor(SensorType.StepCounter);

        sensorManager?.RegisterListener(this, stepCounter, SensorDelay.Ui);
    }

    private partial void Stop()
    {
        sensorManager?.UnregisterListener(this);
    }

    // --------------------------------------------------------------------------------
    // ISensorEventListener
    // --------------------------------------------------------------------------------

    public void OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
    {
    }

    public void OnSensorChanged(SensorEvent? e)
    {
        if (e?.Sensor?.Type == SensorType.StepCounter)
        {
            if (e.Values?.Count > 0)
            {
                if (!started)
                {
                    baseCount = (int)e.Values[0];
                    started = true;
                }

                var count = (int)e.Values[0] - baseCount;
                Changed?.Invoke(this, new ActivityEventArgs(DateTime.Now, count));
            }
        }
    }
}
