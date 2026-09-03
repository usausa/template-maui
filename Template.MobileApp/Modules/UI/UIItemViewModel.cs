namespace Template.MobileApp.Modules.UI;

public sealed partial class UIItemViewModel : AppViewModelBase
{
    public string Title { get; } = "Aqua Serum";

    public string Price { get; } = "$ 24.00";

    public string Category { get; } = "Skin Care";

    public string Description { get; } =
        "A lightweight hydrating serum with hyaluronic acid and marine minerals. Leaves your skin fresh, plump and deeply moisturized.";

    [ObservableProperty]
    public partial string SelectedSize { get; set; } = "50ml";

    [ObservableProperty]
    public partial int Quantity { get; set; } = 1;

    public IObserveCommand BackCommand { get; }

    public IObserveCommand CartCommand { get; }

    public IObserveCommand SizeCommand { get; }

    public IObserveCommand IncrementCommand { get; }

    public IObserveCommand DecrementCommand { get; }

    public UIItemViewModel()
    {
        BackCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIShop));
        CartCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UICart));
        SizeCommand = MakeDelegateCommand<string>(x => SelectedSize = x);
        IncrementCommand = MakeDelegateCommand(() => Quantity = Math.Min(99, Quantity + 1));
        DecrementCommand = MakeDelegateCommand(() => Quantity = Math.Max(1, Quantity - 1));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIShop);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
