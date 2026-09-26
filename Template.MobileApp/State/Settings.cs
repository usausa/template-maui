namespace Template.MobileApp.State;

#pragma warning disable CA1724
public sealed class Settings
{
    private const string AIServiceKeyName = "AIServiceKey";

    private readonly IPreferences preferences;

    private readonly ISecureStorage secureStorage;

    public Settings(IPreferences preferences, ISecureStorage secureStorage)
    {
        this.preferences = preferences;
        this.secureStorage = secureStorage;
    }

    // Id

    public string UniqueId
    {
        get => preferences.Get<string>(nameof(UniqueId), default!);
        set => preferences.Set(nameof(UniqueId), value);
    }

    // API

    public string ApiEndPoint
    {
        get => preferences.Get<string>(nameof(ApiEndPoint), default!);
        set => preferences.Set(nameof(ApiEndPoint), value);
    }

    // gRPC (チャット) の接続先。API と別ポート (h2c) のため別に持つ
    public string GrpcEndPoint
    {
        get => preferences.Get<string>(nameof(GrpcEndPoint), default!);
        set => preferences.Set(nameof(GrpcEndPoint), value);
    }

    // OpenTelemetry (OTLP) の送信先
    public string OtelEndPoint
    {
        get => preferences.Get<string>(nameof(OtelEndPoint), default!);
        set => preferences.Set(nameof(OtelEndPoint), value);
    }

    // テレメトリの送信 (設定画面で切り替える)
    public bool TelemetryEnabled
    {
        get => preferences.Get(nameof(TelemetryEnabled), false);
        set => preferences.Set(nameof(TelemetryEnabled), value);
    }

    // AI Service

    public string AIServiceEndPoint
    {
        get => preferences.Get<string>(nameof(AIServiceEndPoint), default!);
        set => preferences.Set(nameof(AIServiceEndPoint), value);
    }

    // Ollama (Chat)。接続先とモデル名は設定画面の QR で投入する

    public string OllamaEndPoint
    {
        get => preferences.Get<string>(nameof(OllamaEndPoint), default!);
        set => preferences.Set(nameof(OllamaEndPoint), value);
    }

    public string OllamaModel
    {
        get => preferences.Get<string>(nameof(OllamaModel), default!);
        set => preferences.Set(nameof(OllamaModel), value);
    }

    // キー類は平文のPreferencesではなくSecureStorageに保存する
    public async ValueTask<string?> GetAIServiceKeyAsync()
    {
        // 旧バージョンがPreferencesに保存した値はSecureStorageへ移行する
        var legacy = preferences.Get<string?>(AIServiceKeyName, null);
        if (!String.IsNullOrEmpty(legacy))
        {
            await SetSecureValueAsync(AIServiceKeyName, legacy);
            preferences.Remove(AIServiceKeyName);
            return legacy;
        }

        return await GetSecureValueAsync(AIServiceKeyName);
    }

    public ValueTask SetAIServiceKeyAsync(string value)
    {
        // SetAsyncは空文字を受け付けないため、クリアは削除として扱う
        if (String.IsNullOrEmpty(value))
        {
            RemoveSecureValue(AIServiceKeyName);
            return ValueTask.CompletedTask;
        }

        return SetSecureValueAsync(AIServiceKeyName, value);
    }

    // SSH (接続情報は設定画面のQRで投入する。パスワードはSecureStorageに保存する)

    private const string SshPasswordName = "SshPassword";

    public string SshHost
    {
        get => preferences.Get<string>(nameof(SshHost), default!);
        set => preferences.Set(nameof(SshHost), value);
    }

    public int SshPort
    {
        get => preferences.Get(nameof(SshPort), 22);
        set => preferences.Set(nameof(SshPort), value);
    }

    public string SshUser
    {
        get => preferences.Get<string>(nameof(SshUser), default!);
        set => preferences.Set(nameof(SshUser), value);
    }

    public ValueTask<string?> GetSshPasswordAsync() => GetSecureValueAsync(SshPasswordName);

    public ValueTask SetSshPasswordAsync(string value)
    {
        if (String.IsNullOrEmpty(value))
        {
            RemoveSecureValue(SshPasswordName);
            return ValueTask.CompletedTask;
        }

        return SetSecureValueAsync(SshPasswordName, value);
    }

    // ------------------------------------------------------------
    // SecureStorage
    // ------------------------------------------------------------

    // バックアップ復元や端末のロック設定変更でキーストアの鍵が無効になると、復号に失敗して Java 例外になる。
    // 保存済みの値は取り戻せないため、保存領域ごと初期化して未設定として扱う
    private async ValueTask<string?> GetSecureValueAsync(string key)
    {
        try
        {
            return await secureStorage.GetAsync(key);
        }
        catch (Java.Lang.Throwable ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            ResetSecureStorage();
            return null;
        }
    }

    private void RemoveSecureValue(string key)
    {
        try
        {
            secureStorage.Remove(key);
        }
        catch (Java.Lang.Throwable ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            ResetSecureStorage();
        }
    }

    private async ValueTask SetSecureValueAsync(string key, string value)
    {
        try
        {
            await secureStorage.SetAsync(key, value);
        }
        catch (Java.Lang.Throwable ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            ResetSecureStorage();
            await secureStorage.SetAsync(key, value);
        }
    }

    // Remove / RemoveAll も復号を伴って同じ例外になるため、暗号化層を通さずに実体の SharedPreferences を消す
    private static void ResetSecureStorage()
    {
#if ANDROID
        var context = global::Android.App.Application.Context;
        using var preferences = context.GetSharedPreferences($"{context.PackageName}.microsoft.maui.essentials.preferences", global::Android.Content.FileCreationMode.Private)!;
        using var editor = preferences.Edit()!;
        editor.Clear()!.Apply();
#endif
    }
}
#pragma warning restore CA1724

// 通信系の設定が投入済みかの判定 (未設定のときは各機能を実行させない)
public static class SettingsExtensions
{
    // Web API / ストレージ / SignalR
    public static bool IsApiConfigured(this Settings settings) =>
        Uri.TryCreate(settings.ApiEndPoint, UriKind.Absolute, out _);

    // gRPC (チャット)
    public static bool IsGrpcConfigured(this Settings settings) =>
        Uri.TryCreate(settings.GrpcEndPoint, UriKind.Absolute, out _);

    // OpenTelemetry
    public static bool IsOtelConfigured(this Settings settings) =>
        Uri.TryCreate(settings.OtelEndPoint, UriKind.Absolute, out _);

    // テレメトリの送信先 (有効、かつ送信先が設定済みのときだけ)
    public static Uri? GetTelemetryEndPoint(this Settings settings) =>
        settings.TelemetryEnabled && Uri.TryCreate(settings.OtelEndPoint, UriKind.Absolute, out var uri) ? uri : null;

    // Azure AI Vision (キーは SecureStorage)
    public static async ValueTask<bool> IsAIServiceConfiguredAsync(this Settings settings) =>
        !String.IsNullOrEmpty(settings.AIServiceEndPoint) &&
        !String.IsNullOrEmpty(await settings.GetAIServiceKeyAsync().ConfigureAwait(false));

    // Ollama (チャット)
    public static bool IsOllamaConfigured(this Settings settings) =>
        !String.IsNullOrEmpty(settings.OllamaModel) &&
        Uri.TryCreate(settings.OllamaEndPoint, UriKind.Absolute, out _);

    // SSH
    public static bool IsSshConfigured(this Settings settings) =>
        !String.IsNullOrEmpty(settings.SshHost) && !String.IsNullOrEmpty(settings.SshUser);
}
