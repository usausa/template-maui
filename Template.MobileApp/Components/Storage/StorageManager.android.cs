namespace Template.MobileApp.Components.Storage;

public sealed partial class StorageManager
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Ignore")]
    private partial string ResolvePublicFolder() => AndroidHelper.GetExternalFilesDir();
}
