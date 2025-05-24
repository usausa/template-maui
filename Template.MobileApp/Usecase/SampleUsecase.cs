namespace Template.MobileApp.Usecase;

using Template.MobileApp.Components.Storage;

public sealed class SampleUsecase
{
    private readonly IDialog dialog;

    private readonly IStorageManager storageManager;

    private readonly NetworkOperator networkOperator;

    public SampleUsecase(
        IDialog dialog,
        IStorageManager storageManager,
        NetworkOperator networkOperator)
    {
        this.dialog = dialog;
        this.storageManager = storageManager;
        this.networkOperator = networkOperator;
    }

    //--------------------------------------------------------------------------------
    // Simple
    //--------------------------------------------------------------------------------

    public async ValueTask GetServerTimeAsync()
    {
        var result = await networkOperator.ExecuteVerbose(static n => n.GetServerTimeAsync());
        if (result.IsSuccess)
        {
            await dialog.InformationAsync($"Access success.\r\ntime=[{result.Value.DateTime:yyyy/MM/dd HH:mm:ss}]");
        }
    }

    //--------------------------------------------------------------------------------
    // Test
    //--------------------------------------------------------------------------------

    public ValueTask<IResult<object>> GetTestErrorAsync(int code) =>
        networkOperator.ExecuteVerbose(n => n.GetTestErrorAsync(code));

    public ValueTask<IResult<object>> GetTestDelayAsync(int timeout) =>
        networkOperator.ExecuteVerbose(n => n.GetTestDelayAsync(timeout));

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    public async ValueTask GetDataListAsync()
    {
        var result = await networkOperator.ExecuteVerbose(static n => n.GetDataListAsync());
        if (result.IsSuccess)
        {
            await dialog.InformationAsync($"Access success.\r\ncount=[{result.Value.Entries.Length}]");
        }
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
}
