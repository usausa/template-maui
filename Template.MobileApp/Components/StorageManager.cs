namespace Template.MobileApp.Components;

public interface IStorageManager
{
    string PrivateFolder { get; }

    string PublicFolder { get; }
}

public sealed partial class StorageManager : IStorageManager
{
    private readonly IFileSystem fileSystem;

    public string PrivateFolder => fileSystem.AppDataDirectory;

    public string PublicFolder => ResolvePublicFolder();

    public StorageManager(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    private partial string ResolvePublicFolder();
}
