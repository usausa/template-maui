namespace Template.MobileApp.Models.Sample;

public sealed class RadarTarget
{
    // 角度(度 0-360)
    public required float Angle { get; init; }

    // 中心からの距離(0-1)
    public required float Distance { get; init; }
}
