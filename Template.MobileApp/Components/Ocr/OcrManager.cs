namespace Template.MobileApp.Components.Ocr;

public interface IOcrManager
{
    public Task<string?> ReadTextAsync(Stream stream);
}

public sealed partial class OcrManager : IOcrManager
{
    public partial Task<string?> ReadTextAsync(Stream stream);
}
