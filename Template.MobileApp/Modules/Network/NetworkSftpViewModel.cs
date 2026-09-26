namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Usecase;

public sealed partial class NetworkSftpViewModel : AppViewModelBase
{
    private readonly Settings settings;

    private readonly SshUsecase sshUsecase;

    private Action? cancel;

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

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public NetworkSftpViewModel(
        Settings settings,
        SshUsecase sshUsecase)
    {
        this.settings = settings;
        this.sshUsecase = sshUsecase;

        UploadCommand = MakeDelegateCommand(() => _ = ExecuteUploadAsync(), () => !Busy && Configured);
        DownloadCommand = MakeDelegateCommand(() => _ = ExecuteDownloadAsync(), () => !Busy && Configured && !String.IsNullOrEmpty(RemoteFileName));
        CancelCommand = MakeDelegateCommand(() => cancel?.Invoke(), () => Busy);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        Configured = settings.IsSshConfigured();
        HostDisplay = Configured ? $"{settings.SshUser}@{settings.SshHost}:{settings.SshPort}" : "未設定";
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        cancel?.Invoke();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NetworkMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private Task ExecuteUploadAsync() =>
        ExecuteTransferAsync(async token =>
        {
            var result = await sshUsecase.UploadAsync(new Progress<double>(x => Progress = x), token);
            if (result is null)
            {
                return;
            }

            ApplyResult($"アップロード: {result.FileName} ({result.Size:N0} bytes)", result.Transfer, token);
            if (result.Transfer.Success)
            {
                RemoteFileName = result.FileName;
            }
        });

    private Task ExecuteDownloadAsync() =>
        ExecuteTransferAsync(async token =>
        {
            var result = await sshUsecase.DownloadAsync(RemoteFileName, new Progress<double>(x => Progress = x), token);
            ApplyResult($"ダウンロード: {RemoteFileName} ({result.Size:N0} bytes)", result.Transfer, token);
            if (result.Transfer.Success)
            {
                AddLog($"保存先: {result.Path}");
            }
        });

    private async Task ExecuteTransferAsync(Func<CancellationToken, Task> transfer)
    {
        Busy = true;
        Progress = 0d;
        using var cts = new CancellationTokenSource();
        cancel = cts.Cancel;
        try
        {
            await transfer(cts.Token);
        }
        finally
        {
            cancel = null;
            Busy = false;
        }
    }

    private void ApplyResult(string subject, SftpTransferResult result, CancellationToken token)
    {
        if (result.Success)
        {
            Progress = 1d;
            AddLog($"{subject} 完了");
        }
        else
        {
            AddLog(token.IsCancellationRequested ? $"{subject} キャンセルしました" : $"{subject} 失敗: {result.Error}");
        }

        if (!String.IsNullOrEmpty(result.ServerFingerprint))
        {
            ServerFingerprint = result.ServerFingerprint;
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
}
