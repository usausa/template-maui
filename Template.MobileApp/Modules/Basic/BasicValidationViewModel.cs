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

    // 相関検証: Confirm は Password との一致を Compare 属性で検証する
    [ObservableProperty]
    public partial string Password { get; set; } = default!;

    [Compare(nameof(Password), ErrorMessage = "Password と一致しません")]
    [ObservableProperty]
    public partial string Confirm { get; set; } = default!;

    // CommunityToolkit の検証 Behavior 用 (検証は View 側で完結する)
    [ObservableProperty]
    public partial string Email { get; set; } = default!;

    [ObservableProperty]
    public partial string Quantity { get; set; } = default!;

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

        // 相関先 (Password) が変わったら入力済みの Confirm を再検証する
        PropertyChanged += (_, e) =>
        {
            if ((e.PropertyName == nameof(Password)) && !String.IsNullOrEmpty(Confirm))
            {
                Errors.ClearErrors(nameof(Confirm));
                Validate(nameof(Confirm));
            }
        };
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
