namespace Template.MobileApp.Models.Sample;

public sealed class SocialNotificationInfo
{
    public required string Category { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public required double Percent { get; init; }

    // 表示時の時間差スライドイン用ディレイ(ms)
    public int Delay { get; init; }
}
