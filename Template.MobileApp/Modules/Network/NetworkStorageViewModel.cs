namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Components;
using Template.MobileApp.Usecase;

public sealed class StorageEntryItem
{
    public string Name { get; }

    public bool IsDirectory { get; }

    public string SizeText { get; }

    public string LastModifiedText { get; }

    public string Icon => IsDirectory ? "📁" : "📄";

    public StorageEntryItem(string name, bool isDirectory, long? size, DateTime lastModified)
    {
        Name = name;
        IsDirectory = isDirectory;
        SizeText = isDirectory ? string.Empty : FormatSize(size ?? 0);
        LastModifiedText = lastModified.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
    }

    private static string FormatSize(long size) => size switch
    {
        < 1024 => $"{size} B",
        < 1024 * 1024 => $"{size / 1024d:F1} KB",
        _ => $"{size / (1024d * 1024d):F1} MB"
    };
}

public sealed partial class NetworkStorageViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IStorageManager storageManager;

    private readonly NetworkUsecase networkUsecase;

    private Action? cancel;

    [ObservableProperty(NotifyAlso = [nameof(PathDisplay)])]
    public partial string CurrentPath { get; private set; } = string.Empty;

    public string PathDisplay => $"/{CurrentPath}";

    [ObservableProperty]
    public partial StorageEntryItem? SelectedEntry { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; } = true;

    [ObservableProperty]
    public partial bool Transferring { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    public ObservableCollection<StorageEntryItem> Entries { get; } = [];

    public ObservableCollection<string> Logs { get; } = [];

    public IObserveCommand ReloadCommand { get; }
    public IObserveCommand UpCommand { get; }

    public IObserveCommand UploadFileCommand { get; }
    public IObserveCommand UploadPhotoCommand { get; }
    public IObserveCommand DownloadCommand { get; }
    public IObserveCommand CancelCommand { get; }

    public IObserveCommand DeleteCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public NetworkStorageViewModel(
        IDialog dialog,
        IStorageManager storageManager,
        NetworkUsecase networkUsecase)
    {
        this.dialog = dialog;
        this.storageManager = storageManager;
        this.networkUsecase = networkUsecase;

        ReloadCommand = MakeAsyncCommand(ReloadAsync, () => !Loading && !Transferring);
        UpCommand = MakeAsyncCommand(UpAsync, () => !Loading && !Transferring && (CurrentPath.Length > 0));

        UploadFileCommand = MakeDelegateCommand(() => _ = UploadFileAsync(), () => !Loading && !Transferring);
        UploadPhotoCommand = MakeDelegateCommand(() => _ = UploadPhotoAsync(), () => !Loading && !Transferring);
        DownloadCommand = MakeDelegateCommand(() => _ = DownloadAsync(), () => !Loading && !Transferring && SelectedEntry is { IsDirectory: false });
        DeleteCommand = MakeAsyncCommand(DeleteAsync, () => !Loading && !Transferring && (SelectedEntry is not null));
        CancelCommand = MakeDelegateCommand(() => cancel?.Invoke(), () => Transferring);

        SubscribeSelectedEntry(x => _ = OpenSelectedAsync());
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context) => ReloadAsync();

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        cancel?.Invoke();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2() => (Loading || Transferring) ? Task.CompletedTask : ReloadAsync();

    //--------------------------------------------------------------------------------
    // List
    //--------------------------------------------------------------------------------

    private async Task ReloadAsync()
    {
        Loading = true;
        try
        {
            var result = await networkUsecase.GetStorageListAsync(CurrentPath);
            if (result.IsSuccess)
            {
                Entries.Clear();
                foreach (var entry in result.Value.Entries.OrderBy(static x => !x.Directory).ThenBy(static x => x.Name, StringComparer.OrdinalIgnoreCase))
                {
                    Entries.Add(new StorageEntryItem(entry.Name, entry.Directory, entry.Size, entry.LastModified.ToLocalTime()));
                }
                IsEmpty = Entries.Count == 0;
                SelectedEntry = null;
                AddLog($"一覧 /{CurrentPath} {Entries.Count} 件");
            }
        }
        finally
        {
            Loading = false;
        }
    }

    private Task OpenSelectedAsync()
    {
        var entry = SelectedEntry;
        if (entry is not { IsDirectory: true })
        {
            return Task.CompletedTask;
        }

        CurrentPath = $"{CurrentPath}{entry.Name}/";
        return ReloadAsync();
    }

    private Task UpAsync()
    {
        var trimmed = CurrentPath.TrimEnd('/');
        var index = trimmed.LastIndexOf('/');
        CurrentPath = index < 0 ? string.Empty : trimmed[..(index + 1)];
        return ReloadAsync();
    }

    //--------------------------------------------------------------------------------
    // Transfer
    //--------------------------------------------------------------------------------

    private async Task UploadFileAsync()
    {
        var file = await FilePicker.Default.PickAsync();
        if (file is null)
        {
            return;
        }

        await using var stream = await file.OpenReadAsync();
        await UploadAsync(file.FileName, stream);
    }

    private async Task UploadPhotoAsync()
    {
        var files = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
        var file = files.FirstOrDefault();
        if (file is null)
        {
            return;
        }

        await using var stream = await file.OpenReadAsync();
        await UploadAsync(file.FileName, stream);
    }

    private async Task UploadAsync(string name, Stream stream)
    {
        var path = $"{CurrentPath}{name}";
        var result = await TransferAsync(
            $"アップロード開始: {path} ({stream.Length:N0} bytes)",
            t => networkUsecase.UploadStorageAsync(path, stream, x => Progress = x / 100, t));
        if (result.IsSuccess)
        {
            AddLog($"アップロード完了: {path}");
            await ReloadAsync();
        }
    }

    // 選択したファイルを公開フォルダへ
    private async Task DownloadAsync()
    {
        var entry = SelectedEntry;
        if (entry is not { IsDirectory: false })
        {
            return;
        }

        var path = $"{CurrentPath}{entry.Name}";
        var filename = Path.Combine(storageManager.PublicFolder, entry.Name);
        var result = await TransferAsync($"ダウンロード開始: {path}", t => networkUsecase.DownloadStorageAsync(path, filename, x => Progress = x / 100, t));
        if (result.IsSuccess)
        {
            AddLog($"保存先: {filename}");
        }
    }

    private async Task<NetworkResult> TransferAsync(string startMessage, Func<CancellationToken, ValueTask<NetworkResult>> func)
    {
        Transferring = true;
        Progress = 0d;
        using var cts = new CancellationTokenSource();
        cancel = cts.Cancel;
        try
        {
            AddLog(startMessage);
            var result = await func(cts.Token);
            if (result.IsSuccess)
            {
                Progress = 1d;
            }
            else
            {
                AddLog($"転送失敗: {FormatError(result)}");
            }

            return result;
        }
        finally
        {
            cancel = null;
            Transferring = false;
        }
    }

    //--------------------------------------------------------------------------------
    // Delete
    //--------------------------------------------------------------------------------

    private async Task DeleteAsync()
    {
        var entry = SelectedEntry;
        if (entry is null)
        {
            return;
        }

        var path = $"{CurrentPath}{entry.Name}";
        if (!await dialog.ConfirmAsync(entry.IsDirectory ? $"{path}/ を配下ごと削除しますか?" : $"{path} を削除しますか?"))
        {
            return;
        }

        var result = await networkUsecase.DeleteStorageAsync(path);
        if (result.IsSuccess)
        {
            AddLog($"削除: {path}");
            await ReloadAsync();
        }
        else
        {
            AddLog($"削除失敗: {path} {FormatError(result)}");
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static string FormatError(NetworkResult result) =>
        result.Type switch
        {
            NetworkResultType.Disconnected => "未接続",
            NetworkResultType.Canceled => "キャンセル",
            NetworkResultType.NotFound => "見つからない (404)",
            NetworkResultType.HttpError => $"HTTP エラー ({(int)result.StatusCode})",
            _ => "通信エラー"
        };

    private void AddLog(string message)
    {
        Logs.Insert(0, $"{DateTime.Now:HH:mm:ss} {message}");
        while (Logs.Count > 20)
        {
            Logs.RemoveAt(Logs.Count - 1);
        }
    }
}
