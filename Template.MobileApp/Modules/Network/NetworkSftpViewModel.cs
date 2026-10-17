namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Usecase;

public sealed partial class NetworkSftpViewModel : AppViewModelBase
{
    // Field

    private readonly Settings settings;

    private readonly SshUsecase sshUsecase;

    private Action? cancel;

    // Server

    [ObservableProperty]
    public partial string HostDisplay { get; set; } = string.Empty;

    // Transfer

    [ObservableProperty]
    public partial string RemoteFileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial bool Busy { get; set; }

    // Log

    public ObservableCollection<string> Logs { get; } = [];

    // Command

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

        UploadCommand = MakeDelegateCommand(() => _ = UploadAsync(), () => !Busy);
        DownloadCommand = MakeDelegateCommand(() => _ = DownloadAsync(), () => !Busy && !String.IsNullOrEmpty(RemoteFileName));
        CancelCommand = MakeDelegateCommand(() => cancel?.Invoke(), () => Busy);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        HostDisplay = $"{settings.SshUser}@{settings.SshHost}:{settings.SshPort}";
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

    private Task UploadAsync() =>
        TransferAsync(async token =>
        {
            var result = await sshUsecase.UploadAsync(new Progress<double>(x => Progress = x), token);
            if (result is null)
            {
                return;
            }

            ApplyResult($"Upload: {result.FileName} ({result.Size:N0} bytes)", result.Transfer, token);
            if (result.Transfer.Success)
            {
                RemoteFileName = result.FileName;
            }
        });

    private Task DownloadAsync() =>
        TransferAsync(async token =>
        {
            var result = await sshUsecase.DownloadAsync(RemoteFileName, new Progress<double>(x => Progress = x), token);
            ApplyResult($"Download: {RemoteFileName} ({result.Size:N0} bytes)", result.Transfer, token);
            if (result.Transfer.Success)
            {
                AddLog($"Saved: {result.Path}");
            }
        });

    private async Task TransferAsync(Func<CancellationToken, Task> transfer)
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
            AddLog($"{subject} done");
        }
        else
        {
            AddLog(token.IsCancellationRequested ? $"{subject} canceled" : $"{subject} failed: {result.Error}");
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
