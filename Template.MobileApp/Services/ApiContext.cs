namespace Template.MobileApp.Services;

// UIスレッドからの書き込みを通信スレッド(ApiDelegatingHandler)が読むため、参照の可視性をvolatileで保証する
// 401時のトークンリフレッシュはサーバ側仕様が未確定のため未実装 (実案件で実装すること)
public sealed class ApiContext
{
    private volatile Uri? baseAddress;

    private volatile string token = string.Empty;

    public Uri? BaseAddress
    {
        get => baseAddress;
        set => baseAddress = value;
    }

    public string Token
    {
        get => token;
        set => token = value;
    }
}
