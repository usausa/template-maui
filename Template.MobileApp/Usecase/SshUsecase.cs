namespace Template.MobileApp.Usecase;

using System.Net.Sockets;

using Renci.SshNet;
using Renci.SshNet.Common;

using Template.MobileApp.Components;
using Template.MobileApp.Helpers;

public sealed record SftpTransferResult(bool Success, string? Error, string? ServerFingerprint);

public sealed record SftpUploadResult(string FileName, long Size, SftpTransferResult Transfer);

public sealed record SftpDownloadResult(string Path, long Size, SftpTransferResult Transfer);

// 設定の SSH の接続先を使う処理 (SFTP での送受信)
public sealed class SshUsecase
{
    private readonly Settings settings;

    private readonly IStorageManager storageManager;

    public SshUsecase(
        Settings settings,
        IStorageManager storageManager)
    {
        this.settings = settings;
        this.storageManager = storageManager;
    }

    public async ValueTask<SftpUploadResult?> UploadAsync(IProgress<double> progress, CancellationToken cancel)
    {
        var file = await FilePicker.Default.PickAsync();
        if (file is null)
        {
            return null;
        }

        await using var stream = await file.OpenReadAsync();
        var size = stream.Length;
        var result = await ExecuteAsync(
            client => client.UploadFile(new CancellationStream(stream, cancel), file.FileName, uploaded => ReportProgress(progress, uploaded, size)),
            cancel);
        return new SftpUploadResult(file.FileName, size, result);
    }

    public async ValueTask<SftpDownloadResult> DownloadAsync(string remoteFileName, IProgress<double> progress, CancellationToken cancel)
    {
        var path = Path.Combine(storageManager.PublicFolder, Path.GetFileName(remoteFileName));
        SftpTransferResult result;
        long size;
        await using (var stream = File.Create(path))
        {
            result = await ExecuteAsync(
                client =>
                {
                    var total = client.GetAttributes(remoteFileName).Size;
                    client.DownloadFile(remoteFileName, new CancellationStream(stream, cancel), downloaded => ReportProgress(progress, downloaded, total));
                },
                cancel);
            size = stream.Length;
        }

        if (!result.Success)
        {
            File.Delete(path);
        }

        return new SftpDownloadResult(path, size, result);
    }

    private async ValueTask<SftpTransferResult> ExecuteAsync(Action<SftpClient> action, CancellationToken cancel)
    {
        var password = await settings.GetSshPasswordAsync() ?? string.Empty;
        var connectionInfo = new ConnectionInfo(settings.SshHost, settings.SshPort, settings.SshUser, new PasswordAuthenticationMethod(settings.SshUser, password));
        return await Task.Run(
            () =>
            {
                string? serverFingerprint = null;
                try
                {
                    using var client = new SftpClient(connectionInfo);
                    client.HostKeyReceived += (_, e) => serverFingerprint = "SHA256:" + e.FingerPrintSHA256;

                    client.Connect();
                    action(client);
                    client.Disconnect();

                    return new SftpTransferResult(true, null, serverFingerprint);
                }
                catch (Exception ex) when (ex is SshException or SocketException or IOException or InvalidOperationException or ObjectDisposedException or OperationCanceledException)
                {
                    return new SftpTransferResult(false, ex.Message, serverFingerprint);
                }
            },
            cancel);
    }

    private static void ReportProgress(IProgress<double> progress, ulong transferred, long size)
    {
        if (size > 0)
        {
            progress.Report((double)transferred / size);
        }
    }
}
