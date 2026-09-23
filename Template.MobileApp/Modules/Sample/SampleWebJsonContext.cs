namespace Template.MobileApp.Modules.Sample;

using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(WebLocalInfo))]
public partial class SampleWebJsonContext : JsonSerializerContext;
