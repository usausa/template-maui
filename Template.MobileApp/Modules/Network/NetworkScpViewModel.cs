namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Services;

public sealed partial class NetworkScpViewModel : AppViewModelBase
{
    private readonly ScpService scpService;

    private CancellationTokenSource? cts;

    [ObservableProperty]
    public partial bool Configured { get; set; }

    [ObservableProperty]
    public partial string HostDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RemoteFileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial bool Busy { get; set; }

    [ObservableProperty]
    public partial string ServerFingerprint { get; set; } = string.Empty;

    public ObservableCollection<string> Logs { get; } = [];

    public IObserveCommand UploadCommand { get; }

    public IObserveCommand DownloadCommand { get; }

    public IObserveCommand CancelCommand { get; }

    public NetworkScpViewModel(ScpService scpService)
    {
        this.scpService = scpService;

        UploadCommand = MakeAsyncCommand(ExecuteUploadAsync, () => !Busy && Configured);
        DownloadCommand = MakeAsyncCommand(ExecuteDownloadAsync, () => !Busy && Configured && !String.IsNullOrEmpty(RemoteFileName));
        CancelCommand = MakeDelegateCommand(() => cts?.Cancel(), () => Busy);

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(Busy) or nameof(Configured) or nameof(RemoteFileName))
            {
                UploadCommand.RaiseCanExecuteChanged();
                DownloadCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        };
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        Configured = scpService.IsConfigured;
        HostDisplay = Configured ? scpService.HostDisplay : "未設定 (設定画面の QR で投入)";
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        if (cts is not null)
        {
            return cts.CancelAsync();
        }

        return Task.CompletedTask;
    }

    // FilePicker (端末のファイル選択) でアップロード対象を選び、ファイル名のままリモートへ転送する
    private async Task ExecuteUploadAsync()
    {
        var file = await FilePicker.Default.PickAsync();
        if (file is null)
        {
            return;
        }

        Busy = true;
        Progress = 0d;
        var localCts = new CancellationTokenSource();
        cts = localCts;
        try
        {
            await using var stream = await file.OpenReadAsync();
            AddLog($"アップロード開始: {file.FileName} ({stream.Length:N0} bytes)");
            var result = await scpService.UploadAsync(
                stream,
                file.FileName,
                new Progress<double>(x => Progress = x),
                localCts.Token);
            ApplyResult(result);
            if (result.Success)
            {
                RemoteFileName = file.FileName;
            }
        }
        finally
        {
            cts = null;
            localCts.Dispose();
            Busy = false;
        }
    }

    // リモートのファイルをキャッシュディレクトリへ取得する
    private async Task ExecuteDownloadAsync()
    {
        Busy = true;
        Progress = 0d;
        var localCts = new CancellationTokenSource();
        cts = localCts;
        try
        {
            var path = Path.Combine(FileSystem.CacheDirectory, Path.GetFileName(RemoteFileName));
            AddLog($"ダウンロード開始: {RemoteFileName}");
            await using var stream = File.Create(path);
            var result = await scpService.DownloadAsync(
                RemoteFileName,
                stream,
                new Progress<double>(x => Progress = x),
                localCts.Token);
            ApplyResult(result);
            if (result.Success)
            {
                AddLog($"保存先: {path}");
            }
        }
        finally
        {
            cts = null;
            localCts.Dispose();
            Busy = false;
        }
    }

    private void ApplyResult(ScpTransferResult result)
    {
        if (result.Success)
        {
            Progress = 1d;
        }

        AddLog(result.Message);
        if (!String.IsNullOrEmpty(result.ServerFingerprint))
        {
            ServerFingerprint = result.ServerFingerprint;
        }
    }

    private void AddLog(string message)
    {
        Logs.Insert(0, $"{DateTime.Now:HH:mm:ss} {message}");
        while (Logs.Count > 20)
        {
            Logs.RemoveAt(Logs.Count - 1);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            cts?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
