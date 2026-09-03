namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;
using Template.MobileApp.Graphics.Drawing;

public sealed class DeviceActivityViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IActivityRecognizer activityRecognizer;

    public ActivityDrawing Drawing { get; } = new();

    public ActivityCalculator Calculator { get; }

    public DeviceActivityViewModel(
        IDialog dialog,
        IActivityRecognizer activityRecognizer,
        ActivityCalculator activityCalculator)
    {
        this.dialog = dialog;
        this.activityRecognizer = activityRecognizer;
        Calculator = activityCalculator;

        Disposables.Add(activityRecognizer.ChangedAsObservable().ObserveOnCurrentContext()
            .Subscribe(x =>
            {
                Calculator.Update(x.Counter, x.Timestamp);
                Drawing.Step = Calculator.Step;
            }));
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (await Permissions.RequestActivityRecognitionAsync())
        {
            activityRecognizer.Enabled = true;
        }
        else
        {
            await dialog.InformationAsync("Activity recognition permission is not granted.");
        }
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        activityRecognizer.Enabled = false;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
