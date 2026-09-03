namespace Template.MobileApp.Services;

using System.Net.Sockets;

using Renci.SshNet;
using Renci.SshNet.Common;

using Template.MobileApp.State;

public sealed record ScpTransferResult(bool Success, string Message, string? ServerFingerprint);

// SCP 転送サービス (SSH.NET の ScpClient ラッパ)。
// 接続情報は Settings (設定画面の QR で投入) を使う。
// サーバのホスト鍵指紋 (SHA256) は参考情報として結果で返す (照合は行わない)
public sealed class ScpService
{
    private readonly Settings settings;

    public ScpService(Settings settings)
    {
        this.settings = settings;
    }

    public bool IsConfigured =>
        !String.IsNullOrEmpty(settings.ScpHost) && !String.IsNullOrEmpty(settings.ScpUser);

    public string HostDisplay =>
        IsConfigured ? $"{settings.ScpUser}@{settings.ScpHost}:{settings.ScpPort}" : string.Empty;

    public async Task<ScpTransferResult> UploadAsync(Stream source, string remotePath, IProgress<double> progress, CancellationToken cancel)
    {
        var password = await settings.GetScpPasswordAsync().ConfigureAwait(false) ?? string.Empty;

        return await Task.Run(
            () => Execute(
                client =>
                {
                    client.Uploading += (_, e) =>
                    {
                        if (e.Size > 0)
                        {
                            progress.Report((double)e.Uploaded / e.Size);
                        }
                    };
                    client.Upload(source, remotePath);
                    return $"アップロード完了: {remotePath}";
                },
                password,
                cancel),
            cancel).ConfigureAwait(false);
    }

    public async Task<ScpTransferResult> DownloadAsync(string remotePath, Stream destination, IProgress<double> progress, CancellationToken cancel)
    {
        var password = await settings.GetScpPasswordAsync().ConfigureAwait(false) ?? string.Empty;

        return await Task.Run(
            () => Execute(
                client =>
                {
                    client.Downloading += (_, e) =>
                    {
                        if (e.Size > 0)
                        {
                            progress.Report((double)e.Downloaded / e.Size);
                        }
                    };
                    client.Download(remotePath, destination);
                    return $"ダウンロード完了: {remotePath} ({destination.Length:N0} bytes)";
                },
                password,
                cancel),
            cancel).ConfigureAwait(false);
    }

    private ScpTransferResult Execute(Func<ScpClient, string> action, string password, CancellationToken cancel)
    {
        string? serverFingerprint = null;

        try
        {
            // 旧形式コンストラクタ (パス未エスケープ) を避け、ShellQuote でリモートパスを変換する
            var connectionInfo = new ConnectionInfo(settings.ScpHost, settings.ScpPort, settings.ScpUser, new PasswordAuthenticationMethod(settings.ScpUser, password));
            using var client = new ScpClient(connectionInfo, RemotePathTransformation.ShellQuote);
            client.HostKeyReceived += (_, e) => serverFingerprint = "SHA256:" + e.FingerPrintSHA256;

            // キャンセルは切断で反映する (ScpClient の転送 API は同期のため)
            using var registration = cancel.Register(client.Disconnect);

            client.Connect();
            var message = action(client);
            client.Disconnect();

            return new ScpTransferResult(true, message, serverFingerprint);
        }
        catch (Exception ex) when (ex is SshException or SocketException or IOException or InvalidOperationException or ObjectDisposedException or OperationCanceledException)
        {
            if (cancel.IsCancellationRequested)
            {
                return new ScpTransferResult(false, "キャンセルしました", serverFingerprint);
            }

            return new ScpTransferResult(false, $"失敗: {ex.Message}", serverFingerprint);
        }
    }
}
