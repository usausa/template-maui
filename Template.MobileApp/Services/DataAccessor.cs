namespace Template.MobileApp.Services;

using System.Data.Common;

using Smart.Data.Accessor.Attributes;

[DataAccessor]
public sealed partial class DataAccessor
{
    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    [Execute]
    public partial ValueTask<int> ExecutePragmaAsync(DbConnection con);

    [Execute]
    public partial ValueTask<int> CreateTablesAsync(DbConnection con);

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    [Execute]
    [Insert(typeof(DataEntity))]
    public partial ValueTask<int> InsertDataAsync(DataEntity entity);

    [Execute]
    public partial ValueTask<int> UpdateDataAsync(long id, string name);

    [Execute]
    [Delete(typeof(DataEntity))]
    public partial ValueTask<int> DeleteDataAsync(long id);

    [QueryFirst]
    [SelectSingle(typeof(DataEntity))]
    public partial ValueTask<DataEntity?> QueryDataAsync(long id);

    //--------------------------------------------------------------------------------
    // BulkData
    //--------------------------------------------------------------------------------

    [ExecuteScalar]
    [Count(typeof(BulkDataEntity))]
    public partial ValueTask<long> CountBulkDataAsync();

    [Execute]
    [Insert(typeof(BulkDataEntity))]
    public partial int InsertBulkData(DbTransaction tx, BulkDataEntity entity);

    [Execute]
    [Delete(typeof(BulkDataEntity))]
    public partial ValueTask<int> DeleteAllBulkDataAsync();

    [Query]
    public partial IReadOnlyList<BulkDataEntity> QueryAllBulkDataList();

    //--------------------------------------------------------------------------------
    // Work
    //--------------------------------------------------------------------------------

    [Query]
    public partial ValueTask<List<WorkEntity>> QueryWorkListAsync();

    [QueryFirst]
    [SelectSingle(typeof(WorkEntity))]
    public partial ValueTask<WorkEntity?> QueryWorkAsync(long id);

    [Execute]
    [Delete(typeof(WorkEntity))]
    public partial ValueTask<int> DeleteAllWorkAsync(DbTransaction tx);

    [Execute]
    [Insert(typeof(WorkEntity))]
    public partial ValueTask<int> InsertWorkAsync(DbTransaction tx, WorkEntity entity);

    [Execute]
    public partial ValueTask<int> InsertWorkWithNextIdAsync(string name);

    [Execute]
    [Update(typeof(WorkEntity))]
    public partial ValueTask<int> UpdateWorkAsync(WorkEntity entity);

    [Execute]
    [Delete(typeof(WorkEntity))]
    public partial ValueTask<int> DeleteWorkAsync(long id);
}
