namespace Template.MobileApp;

using BunnyTail.EmbeddedBuildProperty;

internal static partial class Variants
{
    [BuildProperty]
    public static partial string DeviceProfile { get; }

    [BuildProperty]
    public static partial string Flavor { get; }

    [BuildProperty]
    public static partial string ApiEndPoint { get; }
}
