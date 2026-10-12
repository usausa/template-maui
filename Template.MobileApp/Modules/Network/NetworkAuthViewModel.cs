namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Services;
using Template.MobileApp.Usecase;

public sealed partial class NetworkAuthViewModel : AppViewModelBase
{
    private const string DummyLoginId = "user";

    private readonly ApiContext apiContext;

    private readonly NetworkUsecase networkUsecase;

    [ObservableProperty]
    public partial string LoginId { get; set; } = DummyLoginId;

    [ObservableProperty]
    public partial bool IsAuthenticated { get; set; }

    [ObservableProperty]
    public partial string AuthenticatedId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? TokenExpires { get; set; }

    public IObserveCommand LoginCommand { get; }
    public IObserveCommand LogoutCommand { get; }

    public IObserveCommand SecureCommand { get; }
    public IObserveCommand InvalidateCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public NetworkAuthViewModel(
        ApiContext apiContext,
        NetworkUsecase networkUsecase)
    {
        this.apiContext = apiContext;
        this.networkUsecase = networkUsecase;

        LoginCommand = MakeAsyncCommand(LoginAsync, () => LoginId.Trim().Length > 0);
        LogoutCommand = MakeDelegateCommand(Logout, () => IsAuthenticated);
        SecureCommand = MakeAsyncCommand(SecureAsync);
        InvalidateCommand = MakeDelegateCommand(Invalidate, () => IsAuthenticated);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        UpdateState();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Login
    //--------------------------------------------------------------------------------

    private async Task LoginAsync()
    {
        await networkUsecase.PostAccountLoginAsync(LoginId.Trim());
        UpdateState();
    }

    private void Logout()
    {
        networkUsecase.AccountLogout();
        UpdateState();
    }

    //--------------------------------------------------------------------------------
    // Secure
    //--------------------------------------------------------------------------------

    private async Task SecureAsync()
    {
        await networkUsecase.GetSecretMessageAsync();
        UpdateState();
    }

    private void Invalidate()
    {
        networkUsecase.InvalidateToken();
        UpdateState();
    }

    //--------------------------------------------------------------------------------
    // State
    //--------------------------------------------------------------------------------

    private void UpdateState()
    {
        IsAuthenticated = apiContext.IsAuthenticated;
        AuthenticatedId = apiContext.IsAuthenticated ? apiContext.LoginId : string.Empty;
        TokenExpires = apiContext.TokenExpires;
    }
}
