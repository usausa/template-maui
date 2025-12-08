namespace Template.MobileApp.State;

public sealed class Settings
{
    private readonly IPreferences preferences;

    public Settings(IPreferences preferences)
    {
        this.preferences = preferences;
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

    public string AIServiceKey
    {
        get => preferences.Get<string>(nameof(AIServiceKey), default!);
        set => preferences.Set(nameof(AIServiceKey), value);
    }
}
