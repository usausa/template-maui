namespace Template.MobileApp.Models.Sample;

public sealed partial class ActivityCalculator : ObservableObject
{
    private static readonly TimeSpan ActivityTimeThreshold = TimeSpan.FromSeconds(10);

    private readonly double caloryPerStep;

    private readonly double weight;

    private readonly double stepLength;

    private DateTime lastUpdateTime;

    [ObservableProperty]
    public partial int Step { get; private set; }

    [ObservableProperty]
    public partial double Distance { get; private set; }

    [ObservableProperty]
    public partial double Calories { get; private set; }

    [ObservableProperty]
    public partial TimeSpan ActivityTime { get; private set; }

    public ActivityCalculator(double caloryPerStep, double weight, double stepLength)
    {
        this.caloryPerStep = caloryPerStep;
        this.weight = weight;
        this.stepLength = stepLength;
    }

    public void Update(int step, DateTime timestamp)
    {
        Step = step;
        Distance = step * stepLength;
        Calories = step * weight * caloryPerStep;
        var timespan = timestamp - lastUpdateTime;
        if ((timespan <= ActivityTimeThreshold) && (timespan >= TimeSpan.Zero))
        {
            ActivityTime += timespan;
        }

        lastUpdateTime = timestamp;
    }
}
