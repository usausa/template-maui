namespace Template.MobileApp.Modules.UI;

public sealed partial class UITicketViewModel : AppViewModelBase
{
    // Field

    private const float BrightDefault = -1f;

    private const float BrightMaximum = 1f;

    private static readonly TimeSpan CarouselSettle = TimeSpan.FromMilliseconds(200);

    private readonly IDispatcher dispatcher;

    private readonly TimeProvider timeProvider;

    private readonly IScreen screen;

    // Property

    public TicketWallet Wallet { get; }

    [ObservableProperty]
    public partial int Position { get; set; }

    [ObservableProperty]
    public partial bool IsBright { get; set; }

    // Command

    public IObserveCommand BrightCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UITicketViewModel(IDispatcher dispatcher, TimeProvider timeProvider, IScreen screen)
    {
        this.dispatcher = dispatcher;
        this.timeProvider = timeProvider;
        this.screen = screen;

        BrightCommand = MakeDelegateCommand(ToggleBright);

        Wallet = TicketSample.LoadWallet(DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Restore();
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        SetState(TicketState.Transferred);
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        var pass = TicketSample.CreatePass(DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime), Wallet.Passes.Count);
        Wallet.Passes.Add(pass);
        dispatcher.DispatchDelayed(CarouselSettle, () => Position = Wallet.Passes.Count - 1);
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        SetState(TicketState.Used);
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void SetState(TicketState state)
    {
        if ((Position >= 0) && (Position < Wallet.Passes.Count))
        {
            var pass = Wallet.Passes[Position];
            pass.State = pass.State == state ? TicketState.Valid : state;
        }
    }

    private void ToggleBright()
    {
        IsBright = !IsBright;
        screen.SetScreenBrightness(IsBright ? BrightMaximum : BrightDefault);
    }

    private void Restore()
    {
        if (IsBright)
        {
            IsBright = false;
            screen.SetScreenBrightness(BrightDefault);
        }
    }
}
