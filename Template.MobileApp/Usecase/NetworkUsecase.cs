namespace Template.MobileApp.Usecase;

using Template.MobileApp.Components;
using Template.MobileApp.Services;

public sealed class NetworkUsecase
{
    private readonly IDialog dialog;

    private readonly IStorageManager storageManager;

    private readonly NetworkOperator networkOperator;

    private readonly ApiContext apiContext;

    public NetworkUsecase(
        IDialog dialog,
        IStorageManager storageManager,
        NetworkOperator networkOperator,
        ApiContext apiContext)
    {
        this.dialog = dialog;
        this.storageManager = storageManager;
        this.networkOperator = networkOperator;
        this.apiContext = apiContext;
    }

    //--------------------------------------------------------------------------------
    // Simple
    //--------------------------------------------------------------------------------

    public async ValueTask GetServerTimeAsync()
    {
        var result = await networkOperator.ExecuteVerbose(static n => n.GetServerTimeAsync());
        if (result.IsSuccess)
        {
            await dialog.InformationAsync($"Get success.\r\ntime=[{result.Value.DateTime:yyyy/MM/dd HH:mm:ss}]");
        }
    }

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    public async ValueTask GetDataListAsync()
    {
        var result = await networkOperator.ExecuteVerbose(static n => n.GetDataListAsync());
        if (result.IsSuccess)
        {
            await dialog.InformationAsync($"Get success.\r\ncount=[{result.Value.Entries.Length}]");
        }
    }

    //--------------------------------------------------------------------------------
    // Secret
    //--------------------------------------------------------------------------------

    public async ValueTask GetSecretMessageAsync()
    {
        var result = await networkOperator.ExecuteVerbose(static n => n.GetSecretMessageAsync());
        if (result.IsSuccess)
        {
            await dialog.InformationAsync($"Get success.\r\nmessage=[{result.Value.Message}]");
        }
    }

    public async ValueTask PostAccountLoginAsync(string id)
    {
        var request = new AccountLoginRequest { Id = id };
        var result = await networkOperator.ExecuteVerbose(n => n.PostAccountLoginAsync(request));
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

    public async ValueTask DownloadAsync()
    {
        var path = Path.Combine(storageManager.PublicFolder, "data.txt");

        // Download
        var result = await networkOperator.ExecuteProgressVerbose(
            (n, p) => n.DownloadAsync("data.txt", path, p.Update));
        if (result == NetworkOperationResult.Success)
        {
            await dialog.InformationAsync("Download success.");
        }
        else if (result == NetworkOperationResult.NotFound)
        {
            await dialog.InformationAsync("Download file not found.");
        }
    }

    public async ValueTask UploadAsync()
    {
        var path = Path.Combine(storageManager.PublicFolder, "data.txt");

        // Make dummy
        if (!File.Exists(path))
        {
            using (dialog.Loading("Make dummy file..."))
            {
                await File.WriteAllLinesAsync(path, Enumerable.Range(1, 100000).Select(static x => $"{x:D10}"));
            }
        }

        // Upload
        var result = await networkOperator.ExecuteProgressVerbose(
            (n, p) => n.UploadAsync("data.txt", path, p.Update));
        if (result == NetworkOperationResult.Success)
        {
            await dialog.InformationAsync("Upload success.");
        }
    }

    //--------------------------------------------------------------------------------
    // Test
    //--------------------------------------------------------------------------------

    public ValueTask<IResult<object>> GetTestErrorAsync(int code) =>
        networkOperator.ExecuteVerbose(n => n.GetTestErrorAsync(code));

    public ValueTask<IResult<object>> GetTestDelayAsync(int timeout) =>
        networkOperator.ExecuteVerbose(n => n.GetTestDelayAsync(timeout));
}
