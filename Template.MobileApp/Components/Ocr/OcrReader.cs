namespace Template.MobileApp.Components.Ocr;

public interface IOcrReader
{
    public Task<string?> ReadTextAsync(Stream stream);
}

public sealed partial class OcrReader : IOcrReader
{
    public partial Task<string?> ReadTextAsync(Stream stream);
}
