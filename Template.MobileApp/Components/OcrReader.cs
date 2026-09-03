namespace Template.MobileApp.Components;

public interface IOcrReader
{
    Task<string?> ReadTextAsync(Stream stream, CancellationToken cancellationToken = default);
}

public sealed partial class OcrReader : IOcrReader
{
    public partial Task<string?> ReadTextAsync(Stream stream, CancellationToken cancellationToken = default);
}
