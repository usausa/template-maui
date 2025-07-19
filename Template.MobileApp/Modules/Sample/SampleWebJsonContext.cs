namespace Template.MobileApp.Modules.Sample;

using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(int))]
public partial class SampleWebJsonContext : JsonSerializerContext
{
}
