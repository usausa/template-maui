namespace Template.MobileApp.Components;

public interface IOcrReader
{
    Task<string?> ReadTextAsync(Stream stream);
}

public sealed partial class OcrReader : IOcrReader
{
    public partial Task<string?> ReadTextAsync(Stream stream);
}
