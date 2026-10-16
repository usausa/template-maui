namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class RadarTarget
{
    public required float Angle { get; init; }

    public required float Distance { get; init; }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

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
