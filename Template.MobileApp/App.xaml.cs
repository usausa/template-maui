namespace Template.MobileApp;

using Template.MobileApp.Modules;

public partial class App
{
    private readonly INavigator navigator;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        navigator = serviceProvider.GetRequiredService<INavigator>();
        MainPage = serviceProvider.GetRequiredService<MainPage>();
    }

    protected override async void OnStart()
    {
        await navigator.ForwardAsync(ViewId.Menu);
    }
}
