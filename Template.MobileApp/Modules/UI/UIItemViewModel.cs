namespace Template.MobileApp.Modules.UI;

public sealed partial class UIItemViewModel : AppViewModelBase
{
    public string Title { get; } = "メカニカルキーボード";

    public string Price { get; } = "¥19,800";

    public string Category { get; } = "キーボード";

    public string Description { get; } =
        "静音メカニカルスイッチを採用したワイヤレスキーボード。有線と Bluetooth の両対応で、最大 3 台の機器をワンタッチで切り替えられます。";

    [ObservableProperty]
    public partial string SelectedSwitch { get; set; } = "茶軸";

    [ObservableProperty]
    public partial int Quantity { get; set; } = 1;

    public IObserveCommand BackCommand { get; }

    public IObserveCommand CartCommand { get; }

    public IObserveCommand SwitchCommand { get; }

    public IObserveCommand IncrementCommand { get; }

    public IObserveCommand DecrementCommand { get; }

    public UIItemViewModel()
    {
        BackCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIShop));
        CartCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UICart));
        SwitchCommand = MakeDelegateCommand<string>(x => SelectedSwitch = x);
        IncrementCommand = MakeDelegateCommand(() => Quantity = Math.Min(99, Quantity + 1));
        DecrementCommand = MakeDelegateCommand(() => Quantity = Math.Max(1, Quantity - 1));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIShop);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
