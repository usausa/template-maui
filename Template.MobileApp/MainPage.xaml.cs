namespace Template.MobileApp;

using Template.MobileApp.Shell;

public sealed partial class MainPage
{
    private readonly ILogger<MainPage> log;

    public MainPage(ILogger<MainPage> log)
    {
        this.log = log;

        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is MainPageViewModel { BusyState.IsBusy: false } context)
        {
            // fire-and-forgetだが例外だけは観測してログに残す
            context.Navigator.NotifyAsync(ShellEvent.Back).ContinueWith(
                t => log.WarnUnhandledNavigationError(t.Exception!),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
        }

        return true;
    }
}
