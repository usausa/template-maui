namespace Template.MobileApp.Components.Storage;

#pragma warning disable CA1822
public sealed partial class StorageManager
{
    private partial string ResolvePublicFolder() => AndroidHelper.GetExternalFilesDir();
}
#pragma warning restore CA1822
