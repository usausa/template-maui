namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class RadarTarget
{
    // 角度(度 0-360)
    public required float Angle { get; init; }

    // 中心からの距離(0-1)
    public required float Distance { get; init; }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// レーダーの目標の見本。4〜7 個を、角度と距離を乱数で置く (乱数は 0 以上 1 未満を返す関数で受け取る)
public static class RadarSample
{
    public static IReadOnlyList<RadarTarget> LoadTargets(Func<double> next)
    {
        var count = 4 + (int)(next() * 4);
        return [.. Enumerable.Range(0, count).Select(_ => new RadarTarget
        {
            Angle = (float)(next() * 360d),
            Distance = (float)((next() * 0.85d) + 0.1d)
        })];
    }
}
