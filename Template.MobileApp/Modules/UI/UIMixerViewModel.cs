namespace Template.MobileApp.Modules.UI;

#pragma warning disable CA1819
#pragma warning disable CA5394
public sealed partial class UIMixerViewModel : AppViewModelBase
{
    private const int RangeCount = 16;

    private readonly IDispatcherTimer timer;

    private readonly Random random = new();

    private int[] currentValues;
    private int[] previousValues;

    [ObservableProperty]
    public partial double KnobValue1 { get; set; } = 50;

    [ObservableProperty]
    public partial double KnobValue3 { get; set; } = 50;

    [ObservableProperty]
    public partial double KnobValue2 { get; set; } = 50;

    [ObservableProperty]
    public partial double SliderValue1 { get; set; } = 50;

    [ObservableProperty]
    public partial double SliderValue3 { get; set; } = 50;

    [ObservableProperty]
    public partial double SliderValue2 { get; set; } = 50;

    [ObservableProperty]
    public partial int[] Values { get; set; }

    public UIMixerViewModel(IDispatcher dispatcher)
    {
        currentValues = new int[RangeCount];
        previousValues = new int[RangeCount];
        Values = new int[RangeCount];

        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(50);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => OnTimerTick()));
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        timer.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    private void OnTimerTick()
    {
        (currentValues, previousValues) = (previousValues, currentValues);

        for (var i = 0; i < previousValues.Length; i++)
        {
            currentValues[i] = Math.Clamp((int)((previousValues[i] * 0.3) + (random.Next(0, RangeCount + 2) * 0.7)), 0, RangeCount);
        }

        Values = currentValues;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
#pragma warning restore CA5394
#pragma warning restore CA1819
