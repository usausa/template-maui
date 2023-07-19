namespace Template.MobileApp.Services;

using Template.MobileApp.Helpers.Data;

using Microsoft.Data.Sqlite;

using Smart.Data;
using Smart.Data.Mapper;
using Smart.Data.Mapper.Builders;

public class DataServiceOptions
{
    public string Path { get; set; } = default!;
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Ignore")]
public class DataService
{
    private readonly DataServiceOptions options;

    private readonly DelegateDbProvider provider;

    public DataService(DataServiceOptions options)
    {
        this.options = options;

        var connectionString = $"Data Source={options.Path}";
        provider = new DelegateDbProvider(() => new SqliteConnection(connectionString));
    }

    public async ValueTask RebuildAsync()
    {
        if (File.Exists(options.Path))
        {
            File.Delete(options.Path);
        }

        await provider.UsingAsync(async con =>
        {
            await con.ExecuteAsync("PRAGMA AUTO_VACUUM=1");
            await con.ExecuteAsync(SqlHelper.MakeCreate<DataEntity>());
            await con.ExecuteAsync(SqlHelper.MakeCreate<BulkDataEntity>());
            await con.ExecuteAsync(SqlHelper.MakeCreate<WorkEntity>());
        });

        await InsertWorkEnumerableAsync(new[]
        {
            new WorkEntity { Id = 1, Name = "Sample-1" },
            new WorkEntity { Id = 2, Name = "Sample-2" },
            new WorkEntity { Id = 3, Name = "Sample-3" },
            new WorkEntity { Id = 4, Name = "Sample-4" }
        });
    }

    //--------------------------------------------------------------------------------
    // CRUD
    //--------------------------------------------------------------------------------

    public async ValueTask<bool> InsertDataAsync(DataEntity entity)
    {
        return await provider.UsingAsync(async con =>
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
    }

    public async ValueTask<int> UpdateDataAsync(long id, string name)
    {
        return await provider.UsingAsync(con =>
            con.ExecuteAsync(
                SqlUpdate<DataEntity>.Set("Name = @Name", "Id = @Id"), // "UPDATE Data SET Name = @Name WHERE Id = @Id",
                new { Id = id, Name = name }));
    }

    public async ValueTask<int> DeleteDataAsync(long id)
    {
        return await provider.UsingAsync(con =>
            con.ExecuteAsync(
                SqlDelete<DataEntity>.ByKey(), // "DELETE FROM Data WHERE Id = @Id",
                new { Id = id }));
    }

    public async ValueTask<DataEntity?> QueryDataAsync(long id)
    {
        return await provider.UsingAsync(con =>
            con.QueryFirstOrDefaultAsync<DataEntity>(
                SqlSelect<DataEntity>.ByKey(), // "SELECT * FROM Data WHERE Id = @Id",
                new { Id = id }));
    }

    // Bulk

    public async ValueTask<int> CountBulkDataAsync()
    {
        return await provider.UsingAsync(con =>
            con.ExecuteScalarAsync<int>(
                SqlCount<BulkDataEntity>.All())); // "SELECT COUNT(*) FROM BulkData"));
    }

    public void InsertBulkDataEnumerable(IEnumerable<BulkDataEntity> source)
    {
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
    }

    public async ValueTask DeleteAllBulkDataAsync()
    {
        await provider.UsingAsync(con => con.ExecuteAsync("DELETE FROM BulkData"));
    }

    public List<BulkDataEntity> QueryAllBulkDataList()
    {
        return provider.Using(con =>
            con.QueryList<BulkDataEntity>(
                SqlSelect<BulkDataEntity>.All())); // "SELECT * FROM BulkData ORDER BY Key1, Key2, Key3"));
    }

    //--------------------------------------------------------------------------------
    // Work
    //--------------------------------------------------------------------------------

    public ValueTask<List<WorkEntity>> QueryWorkListAsync() =>
        provider.Using(con => con.QueryListAsync<WorkEntity>(SqlSelect<WorkEntity>.All()));

    public ValueTask<WorkEntity?> QueryWorkAsync(int id) =>
        provider.Using(con =>
            con.QueryFirstOrDefaultAsync<WorkEntity>(SqlSelect<WorkEntity>.ByKey(), new { Id = id }));

    public ValueTask InsertWorkEnumerableAsync(IEnumerable<WorkEntity> source)
    {
        return provider.UsingTxAsync(async (con, tx) =>
        {
            foreach (var entity in source)
            {
                await con.ExecuteAsync(SqlInsert<WorkEntity>.Values(), entity, tx);
            }

            await tx.CommitAsync();
        });
    }

    public ValueTask InsertWorkAsync(string name) =>
        provider.UsingAsync(async con =>
        {
            var maxId = await con.ExecuteScalarAsync<int>("SELECT MAX(Id) FROM Work");
            await con.ExecuteAsync(SqlInsert<WorkEntity>.Values(), new WorkEntity { Id = maxId + 1, Name = name });
        });

    public ValueTask<int> UpdateWorkAsync(WorkEntity entity) =>
        provider.UsingAsync(con => con.ExecuteAsync(SqlUpdate<WorkEntity>.Set("Name = @Name", "Id = @Id"), entity));

    public ValueTask<int> DeleteWorkAsync(long id) =>
        provider.UsingAsync(con => con.ExecuteAsync(SqlDelete<WorkEntity>.ByKey(), new { Id = id }));
}
