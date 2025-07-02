namespace Template.MobileApp.State;

public sealed class Settings
{
    private readonly IPreferences preferences;

    public Settings(IPreferences preferences)
    {
        this.preferences = preferences;
    }

    // Id

    public string UniqId
    {
        get => preferences.Get<string>(nameof(UniqId), default!);
        set => preferences.Set(nameof(UniqId), value);
    }

    // API

    public string ApiEndPoint
    {
        get => preferences.Get<string>(nameof(ApiEndPoint), default!);
        set => preferences.Set(nameof(ApiEndPoint), value);
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
