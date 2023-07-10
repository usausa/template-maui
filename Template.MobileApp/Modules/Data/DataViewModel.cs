namespace Template.MobileApp.Modules.Data;

using System.Diagnostics;

using MauiComponents;

using Template.MobileApp;
using Template.MobileApp.Models.Entity;
using Template.MobileApp.Services;

public class DataViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly DataService dataService;

    public NotificationValue<int> BulkDataCount { get; } = new();

    public ICommand InsertCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand QueryCommand { get; }

    public ICommand BulkInsertCommand { get; }
    public ICommand DeleteAllCommand { get; }
    public ICommand QueryAllCommand { get; }

    public DataViewModel(
        ApplicationState applicationState,
        IDialog dialog,
        DataService dataService)
        : base(applicationState)
    {
        this.dialog = dialog;
        this.dataService = dataService;

        InsertCommand = MakeAsyncCommand(Insert);
        UpdateCommand = MakeAsyncCommand(Update);
        DeleteCommand = MakeAsyncCommand(Delete);
        QueryCommand = MakeAsyncCommand(Query);

        BulkInsertCommand = MakeAsyncCommand(BulkInsert, () => BulkDataCount.Value == 0).Observe(BulkDataCount);
        DeleteAllCommand = MakeAsyncCommand(DeleteAll, () => BulkDataCount.Value > 0).Observe(BulkDataCount);
        QueryAllCommand = MakeAsyncCommand(QueryAll);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    public override async void OnNavigatingTo(INavigationContext context)
    {
        BulkDataCount.Value = await dataService.CountBulkDataAsync();
    }

    private async Task Insert()
    {
        var ret = await dataService.InsertDataAsync(new DataEntity { Id = 1L, Name = "Data-1", CreateAt = DateTime.Now });

        if (ret)
        {
            await dialog.InformationAsync("Inserted");
        }
        else
        {
            await dialog.InformationAsync("Key duplicate");
        }
    }

    private async Task Update()
    {
        var effect = await dataService.UpdateDataAsync(1L, "Updated");

        await dialog.InformationAsync($"Effect={effect}");
    }

    private async Task Delete()
    {
        var effect = await dataService.DeleteDataAsync(1L);

        await dialog.InformationAsync($"Effect={effect}");
    }

    private async Task Query()
    {
        var entity = await dataService.QueryDataAsync(1L);

        if (entity is not null)
        {
            await dialog.InformationAsync($"Name={entity.Name}\r\nDate={entity.CreateAt:yyyy/MM/dd HH:mm:ss}");
        }
        else
        {
            await dialog.InformationAsync("Not found");
        }
    }

    private async Task BulkInsert()
    {
        var list = Enumerable.Range(1, 10000)
            .Select(x => new BulkDataEntity
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

        BulkDataCount.Value = await dataService.CountBulkDataAsync();

        await dialog.InformationAsync($"Inserted\r\nElapsed={watch.ElapsedMilliseconds}");
    }

    private async Task DeleteAll()
    {
        await dataService.DeleteAllBulkDataAsync();

        BulkDataCount.Value = await dataService.CountBulkDataAsync();
    }

    private async Task QueryAll()
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
