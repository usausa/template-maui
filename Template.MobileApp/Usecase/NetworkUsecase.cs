namespace Template.MobileApp.Usecase;

using Template.MobileApp.Components;
using Template.MobileApp.Services;

public sealed class NetworkUsecase
{
    private readonly IDialog dialog;

    private readonly IStorageManager storageManager;

    private readonly NetworkOperator networkOperator;

    private readonly ApiContext apiContext;

    private readonly DataService dataService;

    public NetworkUsecase(
        IDialog dialog,
        IStorageManager storageManager,
        NetworkOperator networkOperator,
        ApiContext apiContext,
        DataService dataService)
    {
        this.dialog = dialog;
        this.storageManager = storageManager;
        this.networkOperator = networkOperator;
        this.apiContext = apiContext;
        this.dataService = dataService;
    }

    //--------------------------------------------------------------------------------
    // Simple
    //--------------------------------------------------------------------------------

    public async ValueTask GetServerTimeAsync(CancellationToken cancellationToken = default)
    {
        var result = await networkOperator.ExecuteVerbose(static (n, t) => n.GetServerTimeAsync(t), cancellationToken);
        if (result.IsSuccess)
        {
            await dialog.InformationAsync($"Get success.\r\ntime=[{result.Value.DateTime.ToLocalTime():yyyy/MM/dd HH:mm:ss}]");
        }
    }

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    public async ValueTask GetDataListAsync(CancellationToken cancellationToken = default)
    {
        var result = await networkOperator.ExecuteVerbose(static (n, t) => n.GetDataListAsync(t), cancellationToken);
        if (result.IsSuccess)
        {
            // 取得した一覧を Work テーブルへ保存する (Navigation > Edit で確認できる)
            await dataService.ReplaceWorkEnumerableAsync(result.Value.Entries.Select(ObjectMapper.ToWorkEntity));

            await dialog.InformationAsync($"Get success.\r\ncount=[{result.Value.Entries.Length}]\r\nSaved to Work table.");
        }
    }

    //--------------------------------------------------------------------------------
    // Secret
    //--------------------------------------------------------------------------------

    public async ValueTask GetSecretMessageAsync(CancellationToken cancellationToken = default)
    {
        var result = await networkOperator.ExecuteVerbose(static (n, t) => n.GetSecretMessageAsync(t), cancellationToken);
        if (result.IsSuccess)
        {
            await dialog.InformationAsync($"Get success.\r\nmessage=[{result.Value.Message}]");
        }
    }

    public async ValueTask PostAccountLoginAsync(string id, CancellationToken cancellationToken = default)
    {
        var request = new AccountLoginRequest { Id = id };
        var result = await networkOperator.ExecuteVerbose((n, t) => n.PostAccountLoginAsync(request, t), cancellationToken);
        if (result.IsSuccess)
        {
            await dialog.InformationAsync("Login success.");
            apiContext.Token = result.Value.Token;
        }
    }

    public void AccountLogout()
    {
        apiContext.Token = string.Empty;
    }

    //--------------------------------------------------------------------------------
    // Download/Upload
    //--------------------------------------------------------------------------------

    public async ValueTask DownloadAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(storageManager.PublicFolder, "data.txt");

        // Download
        var result = await networkOperator.ExecuteProgressVerbose(
            (n, p, t) => n.DownloadAsync("data.txt", path, p.Update, t), cancellationToken);
        if (result == NetworkOperationResult.Success)
        {
            await dialog.InformationAsync("Download success.");
        }
        else if (result == NetworkOperationResult.NotFound)
        {
            await dialog.InformationAsync("Download file not found.");
        }
    }

    public async ValueTask UploadAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(storageManager.PublicFolder, "data.txt");

        // Make dummy
        if (!File.Exists(path))
        {
            using (dialog.Loading("Make dummy file..."))
            {
                await File.WriteAllLinesAsync(path, Enumerable.Range(1, 100000).Select(static x => $"{x:D10}"), cancellationToken);
            }
        }

        // Upload
        var result = await networkOperator.ExecuteProgressVerbose(
            (n, p, t) => n.UploadAsync("data.txt", path, p.Update, t), cancellationToken);
        if (result == NetworkOperationResult.Success)
        {
            await dialog.InformationAsync("Upload success.");
        }
    }

    //--------------------------------------------------------------------------------
    // Test
    //--------------------------------------------------------------------------------

    public ValueTask<Result<object>> GetTestErrorAsync(int code, CancellationToken cancellationToken = default) =>
        networkOperator.ExecuteVerbose((n, t) => n.GetTestErrorAsync(code, t), cancellationToken);

    public ValueTask<Result<object>> GetTestDelayAsync(int timeout, CancellationToken cancellationToken = default) =>
        networkOperator.ExecuteVerbose((n, t) => n.GetTestDelayAsync(timeout, t), cancellationToken);
}
