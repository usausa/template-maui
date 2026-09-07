namespace Template.MobileApp.Services;

using Microsoft.Data.Sqlite;

using Smart.Data;

#pragma warning disable CA1002
public sealed class DataService
{
    private readonly IDbProvider provider;

    private readonly DataAccessor accessor;

    public DataService(
        IDbProvider provider,
        DataAccessor accessor)
    {
        this.provider = provider;
        this.accessor = accessor;
    }

    public async ValueTask RebuildAsync()
    {
        string dbPath;
        await using (var con = provider.CreateConnection())
        {
            dbPath = con.DataSource;
        }

        // WAL有効時は-wal/-shmも消さないと、異常終了後に旧WALが新しいDBへ適用される
        foreach (var path in new[] { dbPath, $"{dbPath}-wal", $"{dbPath}-shm" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        await provider.UsingAsync(async con =>
        {
            // WALはDBファイルに永続化され以後の全接続に適用される。busy_timeoutは複数スレッド同時アクセス時のSQLITE_BUSY対策
            await accessor.ExecutePragmaAsync(con);
            await accessor.CreateTablesAsync(con);
        });

        await InsertWorkEnumerableAsync(
        [
            new WorkEntity { Id = 1, Name = "Sample-1" },
            new WorkEntity { Id = 2, Name = "Sample-2" },
            new WorkEntity { Id = 3, Name = "Sample-3" },
            new WorkEntity { Id = 4, Name = "Sample-4" }
        ]);
    }

    //--------------------------------------------------------------------------------
    // CRUD
    //--------------------------------------------------------------------------------

    public async ValueTask<bool> InsertDataAsync(DataEntity entity)
    {
        try
        {
            await accessor.InsertDataAsync(entity);

            return true;
        }
        catch (SqliteException e) when (e.SqliteErrorCode == SQLitePCL.raw.SQLITE_CONSTRAINT)
        {
            return false;
        }
    }

    public ValueTask<int> UpdateDataAsync(long id, string name) =>
        accessor.UpdateDataAsync(id, name);

    public ValueTask<int> DeleteDataAsync(long id) =>
        accessor.DeleteDataAsync(id);

    public ValueTask<DataEntity?> QueryDataAsync(long id) =>
        accessor.QueryDataAsync(id);

    // Bulk

    public async ValueTask<int> CountBulkDataAsync() =>
        (int)await accessor.CountBulkDataAsync();

    public void InsertBulkDataEnumerable(IEnumerable<BulkDataEntity> source) =>
        provider.UsingTx((_, tx) =>
        {
            foreach (var entity in source)
            {
                accessor.InsertBulkData(tx, entity);
            }

            tx.Commit();
        });

    public ValueTask<int> DeleteAllBulkDataAsync() =>
        accessor.DeleteAllBulkDataAsync();

    public IReadOnlyList<BulkDataEntity> QueryAllBulkDataList() =>
        accessor.QueryAllBulkDataList();

    //--------------------------------------------------------------------------------
    // Work
    //--------------------------------------------------------------------------------

    public ValueTask<List<WorkEntity>> QueryWorkListAsync() =>
        accessor.QueryWorkListAsync();

    public ValueTask<WorkEntity?> QueryWorkAsync(int id) =>
        accessor.QueryWorkAsync(id);

    public ValueTask InsertWorkEnumerableAsync(IEnumerable<WorkEntity> source) =>
        provider.UsingTxAsync(async (_, tx) =>
        {
            foreach (var entity in source)
            {
                await accessor.InsertWorkAsync(tx, entity);
            }

            await tx.CommitAsync();
        });

    // 洗い替え (既存データを全て削除してから追加する)
    public ValueTask ReplaceWorkEnumerableAsync(IEnumerable<WorkEntity> source) =>
        provider.UsingTxAsync(async (_, tx) =>
        {
            await accessor.DeleteAllWorkAsync(tx);

            foreach (var entity in source)
            {
                await accessor.InsertWorkAsync(tx, entity);
            }

            await tx.CommitAsync();
        });

    // 採番をINSERT単文内で行い、SELECT MAX→INSERTの非アトミック競合を解消
    public async ValueTask InsertWorkAsync(string name) =>
        await accessor.InsertWorkWithNextIdAsync(name);

    public ValueTask<int> UpdateWorkAsync(WorkEntity entity) =>
        accessor.UpdateWorkAsync(entity);

    public ValueTask<int> DeleteWorkAsync(long id) =>
        accessor.DeleteWorkAsync(id);
}
#pragma warning restore CA1002
