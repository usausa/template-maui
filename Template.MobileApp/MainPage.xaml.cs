namespace Template.MobileApp;

using Template.MobileApp.Shell;

public partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        (BindingContext as MainPageViewModel)?.Navigator.NotifyAsync(ShellEvent.Back);
        return true;
    }
}
