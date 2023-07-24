namespace Template.MobileApp.Modules.Basic;

public class BasicStyleViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    public NotificationValue<List<SelectItem?>> Items { get; } = new();

    public NotificationValue<int?> Value1 { get; } = new();
    public NotificationValue<int?> Value2 { get; } = new();
    public NotificationValue<int?> Value3 { get; } = new();

    public ICommand Select1Command { get; }
    public ICommand Select2Command { get; }
    public ICommand Select3Command { get; }

    public BasicStyleViewModel(
        ApplicationState applicationState,
        IDialog dialog)
        : base(applicationState)
    {
        this.dialog = dialog;

        Select1Command = MakeAsyncCommand(async () => Value1.Value = await SelectItem(Value1.Value));
        Select2Command = MakeAsyncCommand(async () => Value2.Value = await SelectItem(Value2.Value));
        Select3Command = MakeAsyncCommand(async () => Value3.Value = await SelectItem(Value3.Value));

        //Items.Value.AddRange(Enumerable.Range(1, 3).Select(x => new SelectItem(x, $"Data-{x}")).Prepend(null));

        Value1.Value = 1;
        Value2.Value = 2;
        Value3.Value = 3;
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
