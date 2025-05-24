namespace Template.MobileApp.Modules.Basic;

public sealed partial class BasicStyleViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    public ObservableCollection<SelectItem?> Items { get; } = new();

    [ObservableProperty]
    public partial int? Value1 { get; set; }
    [ObservableProperty]
    public partial int? Value2 { get; set; }
    [ObservableProperty]
    public partial int? Value3 { get; set; }

    public IObserveCommand Select1Command { get; }
    public IObserveCommand Select2Command { get; }
    public IObserveCommand Select3Command { get; }

    public BasicStyleViewModel(
        IDialog dialog)
    {
        this.dialog = dialog;

        Select1Command = MakeAsyncCommand(async () => Value1 = await SelectItem(Value1));
        Select2Command = MakeAsyncCommand(async () => Value2 = await SelectItem(Value2));
        Select3Command = MakeAsyncCommand(async () => Value3 = await SelectItem(Value3));

        Items.AddRange(Enumerable.Range(1, 3).Select(x => new SelectItem(x, $"Data-{x}")).Prepend(null));

        Value1 = 1;
        Value2 = 2;
        Value3 = 3;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    private async ValueTask<int?> SelectItem(int? current)
    {
        await dialog.InformationAsync("Not implement.");
        return current;
        //var selected = await dialog.SelectAsync(Items.Value, static x => x?.Name ?? string.Empty, Items.Value.FindIndex(x => Equals(x?.Key, current)));
        //return (int?)selected?.Key;
    }
}
