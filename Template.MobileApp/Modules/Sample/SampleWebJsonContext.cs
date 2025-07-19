namespace Template.MobileApp.Modules.Sample;

using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(int))]
internal partial class SampleWebJsonContext : JsonSerializerContext
{
}
