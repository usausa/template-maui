namespace Template.MobileApp.Services;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Template.MobileApp.Components;

public sealed class JsonStore
{
    private readonly string directory;

    public JsonStore(IStorageManager storage)
    {
        directory = Path.Combine(storage.PrivateFolder, "json");
    }

    public T? Load<T>(JsonTypeInfo<T> type)
        where T : class
    {
        var path = GetPath(type);
        T? value = null;
        if (File.Exists(path))
        {
            try
            {
                value = JsonSerializer.Deserialize(File.ReadAllText(path), type);
            }
            catch (JsonException)
            {
                // Ignore broken file
            }
        }

        return value;
    }

    public void Save<T>(T value, JsonTypeInfo<T> type)
    {
        Directory.CreateDirectory(directory);
        var path = GetPath(type);
        var temporary = $"{path}.tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(value, type));
        File.Move(temporary, path, true);
    }

    private string GetPath(JsonTypeInfo type) => Path.Combine(directory, $"{type.Type.Name}.json");
}
