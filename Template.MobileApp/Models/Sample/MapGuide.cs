namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class MapSpot
{
    public string Name { get; init; } = default!;

    public string Description { get; init; } = default!;

    public Location Location { get; init; } = default!;
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// 地図に置く地点の見本 (東京の名所)
public static class MapSample
{
    public static IReadOnlyList<MapSpot> LoadSpots() =>
    [
        new() { Name = "皇居", Description = "千代田区千代田", Location = new Location(35.685175, 139.752800) },
        new() { Name = "東京タワー", Description = "港区芝公園", Location = new Location(35.658581, 139.745433) },
        new() { Name = "東京スカイツリー", Description = "墨田区押上", Location = new Location(35.710063, 139.810700) },
        new() { Name = "浅草寺", Description = "台東区浅草", Location = new Location(35.714765, 139.796655) }
    ];
}
