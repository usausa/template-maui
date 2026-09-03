namespace Template.MobileApp.Services;

using Microsoft.Data.Sqlite;

using Smart.Data;
using Smart.Data.Mapper;
using Smart.Data.Mapper.Builders;

using Template.MobileApp.Helpers.Data;

public sealed class DataServiceOptions
{
    public string Path { get; set; } = default!;
}

#pragma warning disable CA1002
public sealed class DataService
{
    private readonly DataServiceOptions options;

    private readonly DelegateDbProvider provider;

    public DataService(DataServiceOptions options)
    {
        this.options = options;

        // busy_timeoutはPRAGMAだと接続単位になり他接続へ効かないため、接続文字列で全接続に適用する
        var connectionString = $"Data Source={options.Path};Default Timeout=3";
        provider = new DelegateDbProvider(() => new SqliteConnection(connectionString));
    }

    public async ValueTask RebuildAsync()
    {
        // WAL有効時は-wal/-shmも消さないと、異常終了後に旧WALが新しいDBへ適用される
        foreach (var path in new[] { options.Path, $"{options.Path}-wal", $"{options.Path}-shm" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        await provider.UsingAsync(static async con =>
        {
            await con.ExecuteAsync("PRAGMA AUTO_VACUUM=1");
            // WALはDBファイルに永続化され以後の全接続に適用される。busy_timeoutは複数スレッド同時アクセス時のSQLITE_BUSY対策
            await con.ExecuteAsync("PRAGMA journal_mode=WAL");
            await con.ExecuteAsync("PRAGMA busy_timeout=3000");
            await con.ExecuteAsync(SqlHelper.MakeCreate<DataEntity>());
            await con.ExecuteAsync(SqlHelper.MakeCreate<BulkDataEntity>());
            await con.ExecuteAsync(SqlHelper.MakeCreate<WorkEntity>());
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

    public ValueTask<bool> InsertDataAsync(DataEntity entity) =>
        provider.UsingAsync(async con =>
        {
            try
            {
                await con.ExecuteAsync(
                    SqlInsert<DataEntity>.Values(), // "INSERT INTO Data (Id, Name, CreateAt) VALUES (@Id, @Name, @CreateAt)",
                    entity);

                return true;
            }
            catch (SqliteException e)
            {
                if (e.SqliteErrorCode == SQLitePCL.raw.SQLITE_CONSTRAINT)
                {
                    return false;
                }
                throw;
            }
        });

    public ValueTask<int> UpdateDataAsync(long id, string name) =>
        provider.UsingAsync(con =>
            con.ExecuteAsync(
                SqlUpdate<DataEntity>.Set("Name = @Name", "Id = @Id"), // "UPDATE Data SET Name = @Name WHERE Id = @Id",
                new { Id = id, Name = name }));

    public ValueTask<int> DeleteDataAsync(long id) =>
        provider.UsingAsync(con =>
            con.ExecuteAsync(
                SqlDelete<DataEntity>.ByKey(), // "DELETE FROM Data WHERE Id = @Id",
                new { Id = id }));

    public ValueTask<DataEntity?> QueryDataAsync(long id) =>
        provider.UsingAsync(con =>
            con.QueryFirstOrDefaultAsync<DataEntity>(
                SqlSelect<DataEntity>.ByKey(), // "SELECT * FROM Data WHERE Id = @Id",
                new { Id = id }));

    // Bulk

    public ValueTask<int> CountBulkDataAsync() =>
        provider.UsingAsync(static con =>
            con.ExecuteScalarAsync<int>(
                SqlCount<BulkDataEntity>.All())); // "SELECT COUNT(*) FROM BulkData"));

    public void InsertBulkDataEnumerable(IEnumerable<BulkDataEntity> source) =>
        provider.UsingTx((con, tx) =>
        {
            foreach (var entity in source)
            {
                con.Execute(
                    SqlInsert<BulkDataEntity>.Values(), // "INSERT INTO BulkData (Key1, Key2, Key3, Value1, Value2, Value3, Value4, Value5) VALUES (@Key1, @Key2, @Key3, @Value1, @Value2, @Value3, @Value4, @Value5)",
                    entity,
                    tx);
            }

            tx.Commit();
        });

    public ValueTask<int> DeleteAllBulkDataAsync() =>
        provider.UsingAsync(static con => con.ExecuteAsync("DELETE FROM BulkData"));

    public List<BulkDataEntity> QueryAllBulkDataList() =>
        provider.Using(static con =>
            con.QueryList<BulkDataEntity>(
                SqlSelect<BulkDataEntity>.All())); // "SELECT * FROM BulkData ORDER BY Key1, Key2, Key3"));

    //--------------------------------------------------------------------------------
    // Work
    //--------------------------------------------------------------------------------

    public ValueTask<List<WorkEntity>> QueryWorkListAsync() =>
        provider.UsingAsync(static con => con.QueryListAsync<WorkEntity>(SqlSelect<WorkEntity>.All()));

    public ValueTask<WorkEntity?> QueryWorkAsync(int id) =>
        provider.UsingAsync(con =>
            con.QueryFirstOrDefaultAsync<WorkEntity>(SqlSelect<WorkEntity>.ByKey(), new { Id = id }));

    public ValueTask InsertWorkEnumerableAsync(IEnumerable<WorkEntity> source) =>
        provider.UsingTxAsync(async (con, tx) =>
        {
            foreach (var entity in source)
            {
                await con.ExecuteAsync(SqlInsert<WorkEntity>.Values(), entity, tx);
            }

            await tx.CommitAsync();
        });

    public ValueTask InsertWorkAsync(string name) =>
        provider.UsingAsync(async con =>
        {
            // 採番をINSERT単文内で行い、SELECT MAX→INSERTの非アトミック競合を解消
            await con.ExecuteAsync(
                "INSERT INTO Work (Id, Name) VALUES ((SELECT COALESCE(MAX(Id), 0) + 1 FROM Work), @Name)",
                new { Name = name });
        });

    public ValueTask<int> UpdateWorkAsync(WorkEntity entity) =>
        provider.UsingAsync(con => con.ExecuteAsync(SqlUpdate<WorkEntity>.Set("Name = @Name", "Id = @Id"), entity));

    public ValueTask<int> DeleteWorkAsync(long id) =>
        provider.UsingAsync(con => con.ExecuteAsync(SqlDelete<WorkEntity>.ByKey(), new { Id = id }));
}
#pragma warning restore CA1002
