namespace Template.MobileApp.Modules.UI;

public sealed partial class UILoginViewModel : AppViewModelBase
{
    // Property

    [ObservableProperty]
    public partial bool PasswordVisible { get; set; }

    [ObservableProperty]
    public partial bool LoggingIn { get; set; }

    // Command

    public ICommand TogglePasswordCommand { get; }

    public ICommand LoginCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UILoginViewModel()
    {
        TogglePasswordCommand = MakeDelegateCommand(() => PasswordVisible = !PasswordVisible);
        LoginCommand = MakeAsyncCommand(LoginAsync);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private async Task LoginAsync()
    {
        LoggingIn = true;
        try
        {
            await Task.Delay(1500);
        }
        finally
        {
            LoggingIn = false;
        }
    }
}
