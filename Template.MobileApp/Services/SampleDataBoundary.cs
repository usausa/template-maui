namespace Template.MobileApp.Services;

// サンプルデータ生成の下限判定 (過去2ヶ月の月初より前は生成しない)
internal static class SampleDataBoundary
{
    private const int LimitMonths = 2;

    public static bool IsBeforeLimit(DateOnly start)
    {
        var limit = DateOnly.FromDateTime(DateTime.Today).AddMonths(-LimitMonths);
        return start < new DateOnly(limit.Year, limit.Month, 1);
    }
}
