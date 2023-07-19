namespace Template.MobileApp;

using EmbeddedBuildProperty;

internal static partial class Variants
{
    [BuildProperty]
    public static partial string DeviceProfile();

    [BuildProperty]
    public static partial string Flavor();

    [BuildProperty]
    public static partial string AppCenterSecret();

    [BuildProperty]
    public static partial string ApiEndPoint();
}
