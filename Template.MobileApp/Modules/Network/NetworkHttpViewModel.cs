namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Usecase;

public sealed partial class DataListItem : ObservableObject
{
    public long Id { get; }

    [ObservableProperty]
    public partial string Name { get; set; }

    public DataListItem(long id, string name)
    {
        Id = id;
        Name = name;
    }
}

public sealed partial class NetworkHttpViewModel : AppViewModelBase
{
    private const int PageSize = 20;

    private const int DelayMilliseconds = 10000;

    private const int ShortDelayMilliseconds = 5000;

    private const int ErrorStatusCode = 500;

    private readonly IDialog dialog;

    private readonly NetworkUsecase networkUsecase;

    private Action? cancelDelay;

    [ObservableProperty]
    public partial DataListItem? SelectedItem { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Value { get; set; } = "0";

    [ObservableProperty]
    public partial int Total { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; } = true;

    [ObservableProperty]
    public partial bool Delaying { get; set; }

    public ObservableCollection<DataListItem> Items { get; } = [];

    public ObservableCollection<string> Logs { get; } = [];

    public IObserveCommand ReloadCommand { get; }
    public IObserveCommand LoadMoreCommand { get; }

    public IObserveCommand CreateCommand { get; }
    public IObserveCommand UpdateCommand { get; }
    public IObserveCommand DeleteCommand { get; }
    public IObserveCommand ClearCommand { get; }

    public IObserveCommand SaveToWorkCommand { get; }

    public IObserveCommand TestErrorCommand { get; }
    public IObserveCommand TestDelayCommand { get; }

    public IObserveCommand DelayCommand { get; }
    public IObserveCommand CancelCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public NetworkHttpViewModel(
        IDialog dialog,
        NetworkUsecase networkUsecase)
    {
        this.dialog = dialog;
        this.networkUsecase = networkUsecase;

        ReloadCommand = MakeAsyncCommand(ReloadAsync, () => !Loading);
        LoadMoreCommand = MakeAsyncCommand(LoadMoreAsync, () => !Loading);
        CreateCommand = MakeAsyncCommand(CreateAsync, () => !Loading);
        UpdateCommand = MakeAsyncCommand(UpdateAsync, () => !Loading && (SelectedItem is not null));
        DeleteCommand = MakeAsyncCommand(DeleteAsync, () => !Loading && (SelectedItem is not null));
        ClearCommand = MakeDelegateCommand(Clear);
        SaveToWorkCommand = MakeAsyncCommand(async () => await networkUsecase.GetDataListAsync(), () => !Loading);
        TestErrorCommand = MakeAsyncCommand(TestErrorAsync);
        TestDelayCommand = MakeAsyncCommand(TestDelayAsync);

        DelayCommand = MakeDelegateCommand(() => _ = DelayAsync(), () => !Delaying);
        CancelCommand = MakeDelegateCommand(() => cancelDelay?.Invoke(), () => Delaying);

        SubscribeSelectedItem(x => _ = SelectAsync());
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context) => ReloadAsync();

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        cancelDelay?.Invoke();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2() => Loading ? Task.CompletedTask : ReloadAsync();

    //--------------------------------------------------------------------------------
    // List
    //--------------------------------------------------------------------------------

    private async Task ReloadAsync()
    {
        Loading = true;
        try
        {
            var result = await networkUsecase.GetDataRangeAsync(0, PageSize);
            if (result.IsSuccess)
            {
                Items.Clear();
                foreach (var entry in result.Value.Entries)
                {
                    Items.Add(new DataListItem(entry.Id, entry.Name));
                }
                Total = result.Value.Total;
                IsEmpty = Items.Count == 0;
                SelectedItem = null;
                AddLog($"一覧 {Items.Count} / {Total} 件");
            }
        }
        finally
        {
            Loading = false;
        }
    }

    // 残りが少なくなったら次の範囲を読む (サーバーの offset / size)
    private async Task LoadMoreAsync()
    {
        if (Items.Count >= Total)
        {
            return;
        }

        Loading = true;
        try
        {
            var result = await networkUsecase.GetDataRangeAsync(Items.Count, PageSize);
            if (result.IsSuccess)
            {
                foreach (var entry in result.Value.Entries)
                {
                    Items.Add(new DataListItem(entry.Id, entry.Name));
                }
                Total = result.Value.Total;
                AddLog($"追加読み込み {Items.Count} / {Total} 件");
            }
        }
        finally
        {
            Loading = false;
        }
    }

    private async Task SelectAsync()
    {
        var item = SelectedItem;
        if (item is null)
        {
            return;
        }

        var result = await networkUsecase.GetDataAsync(item.Id);
        if (result.IsSuccess)
        {
            Name = result.Value.Name;
            Value = result.Value.Value.ToString(CultureInfo.InvariantCulture);
            AddLog($"取得 id={result.Value.Id} name={result.Value.Name} value={result.Value.Value} created={result.Value.CreatedAt.ToLocalTime():yyyy/MM/dd HH:mm:ss}");
        }
        else
        {
            AddLog($"取得失敗 id={item.Id}: {FormatError(result)}");
        }
    }

    //--------------------------------------------------------------------------------
    // CRUD
    //--------------------------------------------------------------------------------

    private async Task CreateAsync()
    {
        if (!TryGetInput(out var name, out var value))
        {
            return;
        }

        var result = await networkUsecase.CreateDataAsync(name, value);
        if (result.IsSuccess)
        {
            AddLog($"作成 id={result.Value.Id} name={name} value={value}");
            await ReloadAsync();
        }
        else
        {
            AddLog($"作成失敗: {FormatError(result)}");
        }
    }

    private async Task UpdateAsync()
    {
        var item = SelectedItem;
        if ((item is null) || !TryGetInput(out var name, out var value))
        {
            return;
        }

        var result = await networkUsecase.UpdateDataAsync(item.Id, name, value);
        if (result.IsSuccess)
        {
            AddLog($"更新 id={item.Id} name={name} value={value}");
            item.Name = name;
        }
        else
        {
            AddLog($"更新失敗 id={item.Id}: {FormatError(result)}");
        }
    }

    private async Task DeleteAsync()
    {
        var item = SelectedItem;
        if (item is null)
        {
            return;
        }

        if (!await dialog.ConfirmAsync($"id={item.Id} {item.Name} を削除しますか?"))
        {
            return;
        }

        var result = await networkUsecase.DeleteDataAsync(item.Id);
        if (result.IsSuccess)
        {
            AddLog($"削除 id={item.Id}");
            Items.Remove(item);
            Total--;
            IsEmpty = Items.Count == 0;
            SelectedItem = null;
            Clear();
        }
        else
        {
            AddLog($"削除失敗 id={item.Id}: {FormatError(result)}");
        }
    }

    private void Clear()
    {
        SelectedItem = null;
        Name = string.Empty;
        Value = "0";
    }

    private bool TryGetInput(out string name, out int value)
    {
        name = Name.Trim();
        if ((name.Length == 0) || !Int32.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            AddLog("名前と数値を入力してください");
            value = 0;
            return false;
        }

        return true;
    }

    //--------------------------------------------------------------------------------
    // Test
    //--------------------------------------------------------------------------------

    private async Task TestErrorAsync()
    {
        var result = await networkUsecase.GetTestErrorAsync(ErrorStatusCode);
        AddLog($"エラー {ErrorStatusCode}: {FormatError(result)}");
    }

    private async Task TestDelayAsync()
    {
        var result = await networkUsecase.GetTestDelayAsync(ShortDelayMilliseconds);
        AddLog(result.IsSuccess ? $"遅延 {ShortDelayMilliseconds / 1000} 秒 完了" : $"遅延 {ShortDelayMilliseconds / 1000} 秒 失敗: {FormatError(result)}");
    }

    private async Task DelayAsync()
    {
        Delaying = true;
        using var cts = new CancellationTokenSource();
        cancelDelay = cts.Cancel;
        try
        {
            AddLog($"遅延 {DelayMilliseconds / 1000} 秒 開始");
            var result = await networkUsecase.RunTestDelayAsync(DelayMilliseconds, cts.Token);
            AddLog(result.Type switch
            {
                NetworkResultType.Success => "遅延 完了",
                NetworkResultType.Canceled => "遅延 キャンセル",
                _ => $"遅延 失敗: {FormatError(result)}"
            });
        }
        finally
        {
            cancelDelay = null;
            Delaying = false;
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void AddLog(string message)
    {
        Logs.Insert(0, $"{DateTime.Now:HH:mm:ss} {message}");
        while (Logs.Count > 20)
        {
            Logs.RemoveAt(Logs.Count - 1);
        }
    }

    private static string FormatError(NetworkResult result) =>
        result.Type switch
        {
            NetworkResultType.Disconnected => "未接続",
            NetworkResultType.Canceled => "キャンセル",
            NetworkResultType.NotFound => "見つからない (404)",
            NetworkResultType.HttpError => result.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "認証エラー (401)",
                HttpStatusCode.Conflict => "名前が重複 (409)",
                HttpStatusCode.BadRequest => "入力エラー (400)",
                _ => $"HTTP エラー ({(int)result.StatusCode})"
            },
            _ => "通信エラー"
        };
}
