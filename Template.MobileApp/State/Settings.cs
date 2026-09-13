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

    public string MonitorEndPoint
    {
        get => preferences.Get<string>(nameof(MonitorEndPoint), default!);
        set => preferences.Set(nameof(MonitorEndPoint), value);
    }

    // AI Service

    public string AIServiceEndPoint
    {
        get => preferences.Get<string>(nameof(AIServiceEndPoint), default!);
        set => preferences.Set(nameof(AIServiceEndPoint), value);
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

    // SCP (接続情報は設定画面のQRで投入する。パスワードはSecureStorageに保存する)

    private const string ScpPasswordName = "ScpPassword";

    public string ScpHost
    {
        get => preferences.Get<string>(nameof(ScpHost), default!);
        set => preferences.Set(nameof(ScpHost), value);
    }

    public int ScpPort
    {
        get => preferences.Get(nameof(ScpPort), 22);
        set => preferences.Set(nameof(ScpPort), value);
    }

    public string ScpUser
    {
        get => preferences.Get<string>(nameof(ScpUser), default!);
        set => preferences.Set(nameof(ScpUser), value);
    }

    public ValueTask<string?> GetScpPasswordAsync() => GetSecureValueAsync(ScpPasswordName);

    public ValueTask SetScpPasswordAsync(string value)
    {
        if (String.IsNullOrEmpty(value))
        {
            RemoveSecureValue(ScpPasswordName);
            return ValueTask.CompletedTask;
        }

        return SetSecureValueAsync(ScpPasswordName, value);
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
