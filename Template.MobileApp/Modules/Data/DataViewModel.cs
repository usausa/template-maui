namespace Template.MobileApp.Modules.Data;

using System.Diagnostics;

using MauiComponents;

using Template.MobileApp.Services;

public sealed partial class DataViewModel : AppViewModelBase
{
    // Field

    private readonly IDialog dialog;

    private readonly DataService dataService;

    // Property

    [ObservableProperty]
    public partial int BulkDataCount { get; set; }

    // Command

    public IObserveCommand InsertCommand { get; }
    public IObserveCommand UpdateCommand { get; }
    public IObserveCommand DeleteCommand { get; }
    public IObserveCommand QueryCommand { get; }

    public IObserveCommand BulkInsertCommand { get; }
    public IObserveCommand DeleteAllCommand { get; }
    public IObserveCommand QueryAllCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DataViewModel(
        IDialog dialog,
        DataService dataService)
    {
        this.dialog = dialog;
        this.dataService = dataService;

        InsertCommand = MakeAsyncCommand(InsertAsync);
        UpdateCommand = MakeAsyncCommand(UpdateAsync);
        DeleteCommand = MakeAsyncCommand(DeleteAsync);
        QueryCommand = MakeAsyncCommand(QueryAsync);

        BulkInsertCommand = MakeAsyncCommand(BulkInsertAsync, () => BulkDataCount == 0);
        DeleteAllCommand = MakeAsyncCommand(DeleteAllAsync, () => BulkDataCount > 0);
        QueryAllCommand = MakeAsyncCommand(QueryAllAsync);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatingToAsync(INavigationContext context)
    {
        BulkDataCount = await dataService.CountBulkDataAsync();
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private async Task InsertAsync()
    {
        var ret = await dataService.InsertDataAsync(new DataEntity { Id = 1L, Name = "Data-1", CreateAt = DateTime.UtcNow });

        if (ret)
        {
            await dialog.InformationAsync("Inserted");
        }
        else
        {
            await dialog.InformationAsync("Key duplicate");
        }
    }

    private async Task UpdateAsync()
    {
        var effect = await dataService.UpdateDataAsync(1L, "Updated");

        await dialog.InformationAsync($"Effect={effect}");
    }

    private async Task DeleteAsync()
    {
        var effect = await dataService.DeleteDataAsync(1L);

        await dialog.InformationAsync($"Effect={effect}");
    }

    private async Task QueryAsync()
    {
        var entity = await dataService.QueryDataAsync(1L);

        if (entity is not null)
        {
            await dialog.InformationAsync($"Name={entity.Name}\r\nDate={entity.CreateAt.ToLocalTime():yyyy/MM/dd HH:mm:ss}");
        }
        else
        {
            await dialog.InformationAsync("Not found");
        }
    }

    private async Task BulkInsertAsync()
    {
        var list = Enumerable.Range(1, 10000)
            .Select(static x => new BulkDataEntity
            {
                Key1 = $"{x / 1000:D2}",
                Key2 = $"{x % 1000:D2}",
                Key3 = "0",
                Value1 = 1,
                Value2 = 2,
                Value3 = 3,
                Value4 = 4,
                Value5 = 5
            })
            .ToList();

        var watch = new Stopwatch();

        using (dialog.Loading("Inserting..."))
        {
            watch.Start();

            await Task.Run(() => dataService.InsertBulkDataEnumerable(list));

            watch.Stop();
        }

        BulkDataCount = await dataService.CountBulkDataAsync();

        await dialog.InformationAsync($"Inserted\r\nElapsed={watch.ElapsedMilliseconds}");
    }

    private async Task DeleteAllAsync()
    {
        await dataService.DeleteAllBulkDataAsync();

        BulkDataCount = await dataService.CountBulkDataAsync();
    }

    private async Task QueryAllAsync()
    {
        var watch = new Stopwatch();

        using (dialog.Loading())
        {
            watch.Start();

            await Task.Run(dataService.QueryAllBulkDataList);

            watch.Stop();
        }

        await dialog.InformationAsync($"Query\r\nElapsed={watch.ElapsedMilliseconds}");
    }
}
