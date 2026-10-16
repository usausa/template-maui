namespace Template.MobileApp.Services;

using System.Text.Json;

public sealed class ApiContext
{
    private volatile Uri? baseAddress;

    private volatile string token = string.Empty;

    private volatile string loginId = string.Empty;

    public Uri? BaseAddress
    {
        get => baseAddress;
        set => baseAddress = value;
    }

    public string Token => token;

    public string LoginId
    {
        get => loginId;
        set => loginId = value;
    }

    public DateTime? TokenExpires { get; private set; }

    public bool IsAuthenticated => token.Length > 0;

    public void SetToken(string value)
    {
        TokenExpires = GetExpiration(value);
        token = value;
    }

    public void ClearToken()
    {
        token = string.Empty;
        TokenExpires = null;
        loginId = string.Empty;
    }

    private static DateTime? GetExpiration(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + ((4 - (payload.Length % 4)) % 4), '=');
            using var document = JsonDocument.Parse(Convert.FromBase64String(payload));
            if (document.RootElement.TryGetProperty("exp", out var exp) && exp.TryGetInt64(out var seconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(seconds).LocalDateTime;
            }
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            // As expired
        }

        return null;
    }
}
