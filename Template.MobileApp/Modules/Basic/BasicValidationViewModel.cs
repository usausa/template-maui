namespace Template.MobileApp.Modules.Basic;

[GenerateAccessor]
public sealed partial class BasicValidationViewModel : AppViewModelBase
{
    public ValidationFocusRequest ValidationFocusRequest { get; } = new();

    [Required(ErrorMessage = "Required")]
    [ObservableProperty]
    public partial string Text1 { get; set; } = default!;

    [ObservableProperty]
    public partial string Text2 { get; set; } = default!;

    public IObserveCommand ErrorCommand { get; }
    public IObserveCommand ClearCommand { get; }
    public IObserveCommand FocusCommand { get; }

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
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
