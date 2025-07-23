namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;
using Template.MobileApp.Graphics;

public sealed class DeviceActivityViewModel : AppViewModelBase
{
    private readonly IActivityRecognizer activityRecognizer;

    public ActivityGraphics Graphics { get; } = new();

    public ActivityCalculator Calculator { get; }

    public DeviceActivityViewModel(
        IActivityRecognizer activityRecognizer,
        ActivityCalculator activityCalculator)
    {
        this.activityRecognizer = activityRecognizer;
        Calculator = activityCalculator;

        Disposables.Add(activityRecognizer.ChangedAsObservable().ObserveOnCurrentContext()
            .Subscribe(x =>
            {
                Calculator.Update(x.Counter, x.Timestamp);
                Graphics.Step = Calculator.Step;
            }));
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        activityRecognizer.Enabled = true;
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        activityRecognizer.Enabled = false;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
