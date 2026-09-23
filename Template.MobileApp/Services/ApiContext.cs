namespace Template.MobileApp.Services;

using System.Text.Json;

// UIスレッドからの書き込みを通信スレッド(ApiDelegatingHandler)が読むため、参照の可視性をvolatileで保証する
// 401 の再ログインは NetworkUsecase が LoginId で行う (サーバーは Id のみで JWT を発行する契約)
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

    // 最後にログインした Id (空なら未ログイン)
    public string LoginId
    {
        get => loginId;
        set => loginId = value;
    }

    // トークンの有効期限 (JWT の exp。ローカル時刻)
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
