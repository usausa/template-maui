namespace Template.MobileApp.Usecase;

using System.Net.Sockets;

using Renci.SshNet;
using Renci.SshNet.Common;

using Template.MobileApp.Components;
using Template.MobileApp.Helpers;

public sealed record ScpTransferResult(bool Success, string? Error, string? ServerFingerprint);

public sealed record ScpUploadResult(string FileName, long Size, ScpTransferResult Transfer);

public sealed record ScpDownloadResult(string Path, long Size, ScpTransferResult Transfer);

public sealed class ScpUsecase
{
    private readonly Settings settings;

    private readonly IStorageManager storageManager;

    public ScpUsecase(
        Settings settings,
        IStorageManager storageManager)
    {
        this.settings = settings;
        this.storageManager = storageManager;
    }

    public async ValueTask<ScpUploadResult?> UploadAsync(IProgress<double> progress, CancellationToken cancel)
    {
        var file = await FilePicker.Default.PickAsync();
        if (file is null)
        {
            return null;
        }

        await using var stream = await file.OpenReadAsync();
        var result = await ExecuteAsync(
            client =>
            {
                client.Uploading += (_, e) =>
                {
                    if (e.Size > 0)
                    {
                        progress.Report((double)e.Uploaded / e.Size);
                    }
                };
                client.Upload(new CancellationStream(stream, cancel), file.FileName);
            },
            cancel);
        return new ScpUploadResult(file.FileName, stream.Length, result);
    }

    public async ValueTask<ScpDownloadResult> DownloadAsync(string remoteFileName, IProgress<double> progress, CancellationToken cancel)
    {
        var path = Path.Combine(storageManager.PublicFolder, Path.GetFileName(remoteFileName));
        ScpTransferResult result;
        long size;
        await using (var stream = File.Create(path))
        {
            result = await ExecuteAsync(
                client =>
                {
                    client.Downloading += (_, e) =>
                    {
                        if (e.Size > 0)
                        {
                            progress.Report((double)e.Downloaded / e.Size);
                        }
                    };
                    client.Download(remoteFileName, new CancellationStream(stream, cancel));
                },
                cancel);
            size = stream.Length;
        }

        if (!result.Success)
        {
            File.Delete(path);
        }

        return new ScpDownloadResult(path, size, result);
    }

    private async ValueTask<ScpTransferResult> ExecuteAsync(Action<ScpClient> action, CancellationToken cancel)
    {
        var password = await settings.GetScpPasswordAsync() ?? string.Empty;
        var connectionInfo = new ConnectionInfo(settings.ScpHost, settings.ScpPort, settings.ScpUser, new PasswordAuthenticationMethod(settings.ScpUser, password));
        return await Task.Run(
            () =>
            {
                string? serverFingerprint = null;
                try
                {
                    using var client = new ScpClient(connectionInfo, RemotePathTransformation.ShellQuote);
                    client.HostKeyReceived += (_, e) => serverFingerprint = "SHA256:" + e.FingerPrintSHA256;

                    client.Connect();
                    action(client);
                    client.Disconnect();

                    return new ScpTransferResult(true, null, serverFingerprint);
                }
                catch (Exception ex) when (ex is SshException or SocketException or IOException or InvalidOperationException or ObjectDisposedException or OperationCanceledException)
                {
                    return new ScpTransferResult(false, ex.Message, serverFingerprint);
                }
            },
            cancel);
    }
}
