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

    public string TerminalId
    {
        get => preferences.Get<string>(nameof(TerminalId), default!);
        set => preferences.Set(nameof(TerminalId), value);
    }

    // API

    public string ApiEndPoint
    {
        get => preferences.Get<string>(nameof(ApiEndPoint), default!);
        set => preferences.Set(nameof(ApiEndPoint), value);
    }

    // gRPC

    public string GrpcEndPoint
    {
        get => preferences.Get<string>(nameof(GrpcEndPoint), default!);
        set => preferences.Set(nameof(GrpcEndPoint), value);
    }

    // Push
    public bool PushEnabled
    {
        get => preferences.Get(nameof(PushEnabled), false);
        set => preferences.Set(nameof(PushEnabled), value);
    }

    // OpenTelemetry

    public string OtelEndPoint
    {
        get => preferences.Get<string>(nameof(OtelEndPoint), default!);
        set => preferences.Set(nameof(OtelEndPoint), value);
    }

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

    public async ValueTask<string?> GetAIServiceKeyAsync()
    {
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
        if (String.IsNullOrEmpty(value))
        {
            RemoveSecureValue(AIServiceKeyName);
            return ValueTask.CompletedTask;
        }

        return SetSecureValueAsync(AIServiceKeyName, value);
    }

    // SSH

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

    private static void ResetSecureStorage()
    {
#if ANDROID
        var context = Android.App.Application.Context;
        using var preferences = context.GetSharedPreferences($"{context.PackageName}.microsoft.maui.essentials.preferences", Android.Content.FileCreationMode.Private)!;
        using var editor = preferences.Edit()!;
        editor.Clear()!.Apply();
#endif
    }
}
#pragma warning restore CA1724

public static class SettingsExtensions
{
    public static bool IsApiConfigured(this Settings settings) =>
        Uri.TryCreate(settings.ApiEndPoint, UriKind.Absolute, out _);

    public static bool IsGrpcConfigured(this Settings settings) =>
        Uri.TryCreate(settings.GrpcEndPoint, UriKind.Absolute, out _);

    public static bool IsOtelConfigured(this Settings settings) =>
        Uri.TryCreate(settings.OtelEndPoint, UriKind.Absolute, out _);

    public static Uri? GetTelemetryEndPoint(this Settings settings) =>
        settings.TelemetryEnabled && Uri.TryCreate(settings.OtelEndPoint, UriKind.Absolute, out var uri) ? uri : null;

    public static async ValueTask<bool> IsAIServiceConfiguredAsync(this Settings settings) =>
        !String.IsNullOrEmpty(settings.AIServiceEndPoint) &&
        !String.IsNullOrEmpty(await settings.GetAIServiceKeyAsync().ConfigureAwait(false));

    public static bool IsOllamaConfigured(this Settings settings) =>
        !String.IsNullOrEmpty(settings.OllamaModel) &&
        Uri.TryCreate(settings.OllamaEndPoint, UriKind.Absolute, out _);

    public static bool IsSshConfigured(this Settings settings) =>
        !String.IsNullOrEmpty(settings.SshHost) && !String.IsNullOrEmpty(settings.SshUser);
}
