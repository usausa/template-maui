namespace Template.MobileApp.Modules.Data;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using MauiComponents;

using Template.MobileApp.Models;
using Template.MobileApp.Services;

using Smart.ComponentModel;
using Smart.Maui.Input;

public class DataViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly DataService dataService;

    public NotificationValue<int> BulkDataCount { get; } = new();

    public AsyncCommand InsertCommand { get; }
    public AsyncCommand UpdateCommand { get; }
    public AsyncCommand DeleteCommand { get; }
    public AsyncCommand QueryCommand { get; }

    public AsyncCommand BulkInsertCommand { get; }
    public AsyncCommand DeleteAllCommand { get; }
    public AsyncCommand QueryAllCommand { get; }

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

        if (entity != null)
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

            await Task.Run(() => dataService.QueryAllBulkDataList());

            watch.Stop();
        }

        await dialog.InformationAsync($"Query\r\nElapsed={watch.ElapsedMilliseconds}");
    }
}
