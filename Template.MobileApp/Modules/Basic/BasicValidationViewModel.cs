namespace Template.MobileApp.Modules.Basic;

[GenerateAccessor]
public sealed partial class BasicValidationViewModel : AppViewModelBase
{
    // Property

    public ValidationFocusRequest ValidationFocusRequest { get; } = new();

    [Required(ErrorMessage = "Required")]
    [ObservableProperty]
    public partial string Text1 { get; set; } = default!;

    [ObservableProperty]
    public partial string Text2 { get; set; } = default!;

    [ObservableProperty]
    public partial string Password { get; set; } = default!;

    [EqualTo(nameof(Password), ErrorMessage = "Does not match Password")]
    [ObservableProperty]
    public partial string Confirm { get; set; } = default!;

    [ObservableProperty]
    public partial string Email { get; set; } = default!;

    [ObservableProperty]
    public partial string Quantity { get; set; } = default!;

    // Command

    public IObserveCommand ErrorCommand { get; }
    public IObserveCommand ClearCommand { get; }
    public IObserveCommand FocusCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public BasicValidationViewModel()
    {
        ErrorCommand = MakeDelegateCommand(() =>
        {
            Errors.AddError(nameof(Text2), "Manual error");
        });
        ClearCommand = MakeDelegateCommand(() =>
        {
            Errors.ClearErrors(nameof(Text2));
        });
        FocusCommand = MakeDelegateCommand(ValidationFocusRequest.FocusRequest);

        SubscribeConfirm(_ => ValidateConfirm());
        SubscribePassword(_ =>
        {
            if (!String.IsNullOrEmpty(Confirm))
            {
                ValidateConfirm();
            }
        });
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ValidateConfirm()
    {
        Errors.ClearErrors(nameof(Confirm));
        Validate(nameof(Confirm));
    }
}
