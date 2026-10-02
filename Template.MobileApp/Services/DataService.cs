namespace Template.MobileApp.Services;

using Microsoft.Data.Sqlite;

using Smart.Data;

public sealed record DatabaseInfo(
    string Path,
    long Size,
    DateTime? Modified,
    int DataCount,
    int WorkCount,
    int BulkDataCount);

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

    public string DatabasePath
    {
        get
        {
            using var con = provider.CreateConnection();
            return con.DataSource;
        }
    }

    public async ValueTask<DatabaseInfo> GetDatabaseInfoAsync()
    {
        var path = DatabasePath;
        var file = new FileInfo(path);
        var wal = new FileInfo($"{path}-wal");
        return new DatabaseInfo(
            path,
            (file.Exists ? file.Length : 0) + (wal.Exists ? wal.Length : 0),
            file.Exists ? file.LastWriteTime : null,
            (int)await accessor.CountDataAsync(),
            (int)await accessor.CountWorkAsync(),
            (int)await accessor.CountBulkDataAsync());
    }

    public ValueTask RebuildAsync()
    {
        var dbPath = DatabasePath;

        // Delete with WAL and SHM files
        foreach (var path in new[] { dbPath, $"{dbPath}-wal", $"{dbPath}-shm" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        return provider.UsingAsync(async con =>
        {
            await accessor.ExecutePragmaAsync(con);
            await accessor.CreateTablesAsync(con);
        });
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

    public async ValueTask InsertWorkAsync(string name) =>
        await accessor.InsertWorkWithNextIdAsync(name);

    public ValueTask<int> UpdateWorkAsync(WorkEntity entity) =>
        accessor.UpdateWorkAsync(entity);

    public ValueTask<int> DeleteWorkAsync(long id) =>
        accessor.DeleteWorkAsync(id);

    //--------------------------------------------------------------------------------
    // Application
    //--------------------------------------------------------------------------------

    public ValueTask<List<TodoEntity>> QueryTodoListAsync() =>
        accessor.QueryTodoListAsync();

    public ValueTask<long> InsertTodoAsync(TodoEntity entity) =>
        accessor.InsertTodoAsync(entity.Title, entity.Note, entity.DueDate, entity.IsImportant, entity.IsDone, entity.CreatedAt, entity.UpdatedAt);

    public ValueTask InsertTodoEnumerableAsync(IEnumerable<TodoEntity> source) =>
        provider.UsingTxAsync(async (_, tx) =>
        {
            foreach (var entity in source)
            {
                await accessor.InsertTodoEntityAsync(tx, entity);
            }

            await tx.CommitAsync();
        });

    public ValueTask<int> UpdateTodoAsync(TodoEntity entity) =>
        accessor.UpdateTodoAsync(entity);

    public ValueTask<int> DeleteTodoAsync(long id) =>
        accessor.DeleteTodoAsync(id);
}
#pragma warning restore CA1002
