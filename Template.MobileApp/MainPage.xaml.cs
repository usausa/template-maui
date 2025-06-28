namespace Template.MobileApp;

using Template.MobileApp.Shell;

public sealed partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is MainPageViewModel { BusyState.IsBusy: false } context)
        {
            using (context.BusyState.Begin())
            {
                context.Navigator.NotifyAsync(ShellEvent.Back);
            }
        }

        return true;
    }
}
