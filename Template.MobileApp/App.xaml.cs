namespace Template.MobileApp;

using Template.MobileApp.Modules;

public partial class App
{
    private readonly INavigator navigator;

    public App(IServiceProvider serviceProvider, ILogger<App> log)
    {
        InitializeComponent();

        navigator = serviceProvider.GetRequiredService<INavigator>();
        MainPage = serviceProvider.GetRequiredService<MainPage>();

        log.InfoApplicationStart();
    }

    protected override async void OnStart()
    {
        await navigator.ForwardAsync(ViewId.Menu);
    }
}
