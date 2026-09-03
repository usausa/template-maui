namespace Template.MobileApp.Modules.UI;

public sealed partial class UIPosViewModel : AppViewModelBase
{
    private const int UnitPrice = 296;

    [ObservableProperty(NotifyAlso = [nameof(Subtotal), nameof(Total), nameof(InnerTax), nameof(Change), nameof(HasChange)])]
    public partial int Quantity { get; set; }

    public int Subtotal => UnitPrice * Quantity;

    public int Total => Subtotal;

    public int InnerTax => (int)(Total * 8d / 108d);

    public int Deposit { get; } = 888;

    public int Change => Math.Max(0, Deposit - Total);

    public bool HasChange => Change > 0;

    public ICommand QuantityUpCommand { get; }

    public ICommand QuantityDownCommand { get; }

    public UIPosViewModel()
    {
        Quantity = 3;

        QuantityUpCommand = MakeDelegateCommand(() => Quantity = Math.Min(9, Quantity + 1));
        QuantityDownCommand = MakeDelegateCommand(() => Quantity = Math.Max(1, Quantity - 1));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
